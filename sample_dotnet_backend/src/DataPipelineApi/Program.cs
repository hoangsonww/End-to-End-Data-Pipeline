using System.Linq;
using System.Text.Json;
using DataPipelineApi.HealthChecks;
using DataPipelineApi.Options;
using DataPipelineApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<DatabaseOptions>().Bind(builder.Configuration.GetSection("ConnectionStrings")).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<MinioOptions>().Bind(builder.Configuration.GetSection("Minio")).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<KafkaOptions>().Bind(builder.Configuration.GetSection("Kafka")).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<AirflowOptions>().Bind(builder.Configuration.GetSection("Airflow")).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<GEOptions>().Bind(builder.Configuration.GetSection("GreatExpectations")).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<AtlasOptions>().Bind(builder.Configuration.GetSection("Atlas")).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<MLflowOptions>().Bind(builder.Configuration.GetSection("MLflow")).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<GitHubOptions>().Bind(builder.Configuration.GetSection("GitHub")).ValidateDataAnnotations().ValidateOnStart();

var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
  .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));

builder.Services.AddHttpClient<IBatchService, BatchService>((sp, http) =>
{
  var opt = sp.GetRequiredService<IOptions<AirflowOptions>>().Value;
  http.BaseAddress = new Uri(opt.BaseUrl);
  http.Timeout = TimeSpan.FromSeconds(opt.RequestTimeoutSeconds);
  http.DefaultRequestHeaders.UserAgent.ParseAdd("DataPipelineApi/1.0");
}).AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<IStreamingService, StreamingService>((sp, http) =>
{
  var opt = sp.GetRequiredService<IOptions<AirflowOptions>>().Value;
  http.BaseAddress = new Uri(opt.BaseUrl);
  http.Timeout = TimeSpan.FromSeconds(opt.RequestTimeoutSeconds);
  http.DefaultRequestHeaders.UserAgent.ParseAdd("DataPipelineApi/1.0");
}).AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<IAtlasService, AtlasService>((sp, http) =>
{
  var opt = sp.GetRequiredService<IOptions<AtlasOptions>>().Value;
  http.BaseAddress = new Uri(opt.Endpoint);
  http.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<IMLflowService, MLflowService>((sp, http) =>
{
  var opt = sp.GetRequiredService<IOptions<MLflowOptions>>().Value;
  http.BaseAddress = new Uri(opt.TrackingUri);
  http.Timeout = TimeSpan.FromSeconds(opt.RequestTimeoutSeconds);
}).AddPolicyHandler(retryPolicy);
builder.Services.AddHttpClient<ICIService, CIService>((sp, http) =>
{
  var opt = sp.GetRequiredService<IOptions<GitHubOptions>>().Value;
  http.Timeout = TimeSpan.FromSeconds(30);
  http.DefaultRequestHeaders.UserAgent.ParseAdd(opt.UserAgent);
});

builder.Services.AddHttpClient("airflow-health", (sp, http) =>
{
  var opt = sp.GetRequiredService<IOptions<AirflowOptions>>().Value;
  http.BaseAddress = new Uri(opt.BaseUrl);
  http.Timeout = TimeSpan.FromSeconds(opt.RequestTimeoutSeconds);
  var tok = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{opt.Username}:{opt.Password}"));
  http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", tok);
});
builder.Services.AddHttpClient("mlflow-health", (sp, http) =>
{
  var opt = sp.GetRequiredService<IOptions<MLflowOptions>>().Value;
  http.BaseAddress = new Uri(opt.TrackingUri);
  http.Timeout = TimeSpan.FromSeconds(opt.RequestTimeoutSeconds);
});

builder.Services.AddSingleton<IDbService, DbService>();
builder.Services.AddSingleton<IStorageService, MinioService>();
builder.Services.AddSingleton<IKafkaService, KafkaService>();
builder.Services.AddSingleton<IGEValidationService, GEValidationService>();
builder.Services.AddSingleton<IMonitoringService, MonitoringService>();

builder.Services.AddHealthChecks()
  .AddCheck<MySqlHealthCheck>("mysql")
  .AddCheck<PostgresHealthCheck>("postgres")
  .AddCheck<MinioHealthCheck>("minio")
  .AddCheck<KafkaHealthCheck>("kafka")
  .AddCheck<AirflowHealthCheck>("airflow")
  .AddCheck<MLflowHealthCheck>("mlflow");

builder.Services.AddHttpLogging(logging =>
{
  logging.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode;
});

builder.Services.AddResponseCompression();

builder.Services.AddControllers().AddNewtonsoftJson()
  .ConfigureApiBehaviorOptions(options =>
  {
    options.InvalidModelStateResponseFactory = context =>
    {
      var details = new ValidationProblemDetails(context.ModelState)
      {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid request payload"
      };
      return new BadRequestObjectResult(details);
    };
  });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Full Data Pipeline API", Version = "v1" });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseExceptionHandler(errorApp =>
{
  errorApp.Run(async context =>
  {
    var problem = new ProblemDetails
    {
      Status = StatusCodes.Status500InternalServerError,
      Title = "Unexpected error",
      Detail = "An unexpected error occurred while processing the request",
      Instance = context.Request.Path
    };
    problem.Extensions["traceId"] = context.TraceIdentifier;
    context.Response.StatusCode = problem.Status.Value;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(problem);
  });
});

if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseResponseCompression();

app.Use(async (context, next) =>
{
  var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
  context.Response.Headers["X-Request-ID"] = requestId;
  using (app.Logger.BeginScope(new Dictionary<string, object> { { "RequestId", requestId }, { "TraceId", context.TraceIdentifier } }))
  {
    await next();
  }
});
app.UseRouting();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
  ResponseWriter = async (ctx, report) =>
  {
    ctx.Response.ContentType = "application/json";
    var payload = new
    {
      status = report.Status.ToString(),
      results = report.Entries.Select(e => new
      {
        key = e.Key,
        status = e.Value.Status.ToString(),
        description = e.Value.Description
      })
    };
    await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
  }
});

app.Run();
