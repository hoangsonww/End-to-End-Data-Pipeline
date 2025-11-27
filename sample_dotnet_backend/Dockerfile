FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/DataPipelineApi/DataPipelineApi.csproj","DataPipelineApi/"]
RUN dotnet restore "DataPipelineApi/DataPipelineApi.csproj"
COPY src/DataPipelineApi/. ./DataPipelineApi
COPY appsettings.json DataPipelineApi/appsettings.json
WORKDIR /src/DataPipelineApi
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet","DataPipelineApi.dll"]
