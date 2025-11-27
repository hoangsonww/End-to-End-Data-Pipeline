# Quick Start Guide - Advanced Deployments

This guide provides quick commands for common deployment operations.

## Initial Setup

### 1. Install Prerequisites

```bash
# Install kubectl
curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
chmod +x kubectl
sudo mv kubectl /usr/local/bin/

# Install helm
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Install AWS CLI (for AWS deployments)
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install
```

### 2. Setup Infrastructure

```bash
cd scripts
chmod +x *.sh
./setup-advanced-deployments.sh
```

### 3. Configure Terraform (Optional)

```bash
cd terraform
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
terraform init
terraform plan
terraform apply
```

---

## Deployment Commands

### Blue/Green Deployment

```bash
# Deploy new version to preview
./scripts/deploy-blue-green.sh airflow v1.0.0

# Inside the script, you can:
# - View preview endpoint
# - Test preview environment
# - Compare Blue vs Green metrics
# - Promote to production
# - Rollback to Blue
```

### Canary Deployment

```bash
# Deploy with progressive traffic shifting
./scripts/deploy-canary.sh airflow v1.0.0

# The script will:
# - Deploy canary version
# - Progressively shift traffic (10% → 25% → 50% → 75% → 100%)
# - Run automated analysis at each step
# - Auto-rollback on failures
```

---

## Manual Operations

### Using kubectl-argo-rollouts

```bash
# List all rollouts
kubectl argo rollouts list

# Get rollout status
kubectl argo rollouts get rollout airflow-rollout

# Watch rollout progress
kubectl argo rollouts get rollout airflow-rollout --watch

# Promote rollout
kubectl argo rollouts promote airflow-rollout

# Abort rollout
kubectl argo rollouts abort airflow-rollout

# Rollback to previous version
kubectl argo rollouts undo airflow-rollout

# Set new image
kubectl argo rollouts set image airflow-rollout \
  airflow-webserver=myrepo/airflow-pipeline:v2.0.0
```

### Using kubectl

```bash
# Apply rollout manifests
kubectl apply -f kubernetes/rollout-blue-green.yaml
kubectl apply -f kubernetes/rollout-canary.yaml

# Get rollouts
kubectl get rollouts

# Describe rollout
kubectl describe rollout airflow-rollout

# Get analysis runs
kubectl get analysisruns

# View analysis details
kubectl describe analysisrun <analysis-run-name>

# Get pods
kubectl get pods -l app=pipeline
```

---

## Monitoring

### Access Dashboards

```bash
# Argo Rollouts Dashboard
kubectl port-forward -n argo-rollouts svc/argo-rollouts-dashboard 3100:3100
# Open: http://localhost:3100

# Grafana
kubectl port-forward -n monitoring svc/kube-prometheus-grafana 3000:80
# Open: http://localhost:3000 (admin/admin)

# Prometheus
kubectl port-forward -n monitoring svc/kube-prometheus-prometheus 9090:9090
# Open: http://localhost:9090
```

### View Metrics

```bash
# Check success rate
kubectl exec -n monitoring prometheus-0 -- \
  wget -qO- 'http://localhost:9090/api/v1/query?query=service:http_requests:success_rate'

# Check error rate
kubectl exec -n monitoring prometheus-0 -- \
  wget -qO- 'http://localhost:9090/api/v1/query?query=service:http_requests:error_rate'

# Check latency
kubectl exec -n monitoring prometheus-0 -- \
  wget -qO- 'http://localhost:9090/api/v1/query?query=service:http_request_duration:p95'
```

---

## Troubleshooting

### Check Rollout Status

```bash
kubectl argo rollouts get rollout <rollout-name>
kubectl describe rollout <rollout-name>
kubectl get events --sort-by=.metadata.creationTimestamp
```

### Check Analysis

```bash
kubectl get analysisruns
kubectl describe analysisrun <analysis-run-name>
kubectl logs -n argo-rollouts deployment/argo-rollouts
```

### Check Services

```bash
kubectl get svc
kubectl describe svc <service-name>
kubectl get endpoints <service-name>
```

### Check Pods

```bash
kubectl get pods -l app=pipeline
kubectl describe pod <pod-name>
kubectl logs <pod-name>
kubectl logs <pod-name> --previous  # Previous container logs
```

### Emergency Rollback

```bash
# Method 1: Using Argo Rollouts
kubectl argo rollouts abort <rollout-name>
kubectl argo rollouts undo <rollout-name>

# Method 2: Using kubectl
kubectl rollout undo rollout/<rollout-name>

# Method 3: Revert to specific revision
kubectl argo rollouts undo <rollout-name> --to-revision=2
```

---

## Common Scenarios

### Scenario 1: Deploy New Version with Auto-Promotion

```bash
# Apply canary rollout with auto-promotion
kubectl apply -f kubernetes/rollout-canary.yaml

# Update image (deployment starts automatically)
kubectl argo rollouts set image airflow-canary-rollout \
  airflow-webserver=myrepo/airflow-pipeline:v2.0.0

# Watch progress (auto-promotes if analysis passes)
kubectl argo rollouts get rollout airflow-canary-rollout --watch
```

### Scenario 2: Deploy with Manual Approval Gates

```bash
# Deploy blue/green with manual promotion
kubectl apply -f kubernetes/rollout-blue-green.yaml

# Update to new version
kubectl argo rollouts set image airflow-rollout \
  airflow-webserver=myrepo/airflow-pipeline:v2.0.0

# Test preview environment
kubectl port-forward svc/airflow-webserver-preview 8081:8080

# After testing, promote manually
kubectl argo rollouts promote airflow-rollout
```

### Scenario 3: A/B Testing with Header Routing

```bash
# Apply header-based canary
kubectl apply -f kubernetes/rollout-canary.yaml

# Internal users get canary version
curl -H "X-Version: canary" http://your-service.com

# Regular users get stable version
curl http://your-service.com
```

### Scenario 4: Emergency Rollback

```bash
# Immediate abort
kubectl argo rollouts abort airflow-rollout

# Rollback to previous stable version
kubectl argo rollouts undo airflow-rollout

# Verify rollback
kubectl argo rollouts get rollout airflow-rollout
```

---

## Configuration Files

### Key Files and Their Purpose

| File | Purpose |
|------|---------|
| `kubernetes/rollout-blue-green.yaml` | Blue/Green rollout definitions |
| `kubernetes/rollout-canary.yaml` | Canary rollout definitions |
| `kubernetes/analysis-templates.yaml` | Prometheus metrics analysis |
| `kubernetes/services.yaml` | Kubernetes services |
| `kubernetes/ingress.yaml` | Traffic routing rules |
| `scripts/deploy-blue-green.sh` | Interactive blue/green deployment |
| `scripts/deploy-canary.sh` | Interactive canary deployment |
| `scripts/setup-advanced-deployments.sh` | Infrastructure setup |

### Customize Analysis Templates

Edit `kubernetes/analysis-templates.yaml`:

```yaml
# Example: Change success rate threshold
metrics:
  - name: success-rate
    successCondition: result >= 0.99  # Change from 0.95 to 0.99
    failureLimit: 2                    # Reduce from 3 to 2
```

### Customize Traffic Weights

Edit `kubernetes/rollout-canary.yaml`:

```yaml
steps:
  - setWeight: 5    # Start with 5% instead of 10%
  - pause: {duration: 1m}
  - setWeight: 15   # Then 15%
  - pause: {duration: 2m}
  # ... customize as needed
```

---

## Health Checks

### Verify Installation

```bash
# Check Argo Rollouts
kubectl get pods -n argo-rollouts

# Check Prometheus
kubectl get pods -n monitoring

# Check Ingress Controller
kubectl get pods -n ingress-nginx

# Verify all components
kubectl get all -n argo-rollouts
kubectl get all -n monitoring
```

### Test Connectivity

```bash
# Test Prometheus
kubectl exec -n monitoring prometheus-0 -- wget -qO- http://localhost:9090/-/healthy

# Test Grafana
kubectl exec -n monitoring deployment/kube-prometheus-grafana -- wget -qO- http://localhost:3000/api/health

# Test Argo Rollouts
kubectl exec -n argo-rollouts deployment/argo-rollouts -- wget -qO- http://localhost:8080/healthz
```

---

## Performance Tuning

### Optimize Canary Steps

For high-traffic services:
- Start with smaller weights (5%)
- Use shorter soak times (1-2 minutes)
- More granular steps (5%, 10%, 20%, 30%, 50%, 100%)

For critical services:
- Larger initial weight (20%)
- Longer soak times (5-10 minutes)
- Fewer steps (20%, 50%, 100%)

### Optimize Analysis

```yaml
# Fast-moving services
interval: 15s
count: 4        # 1 minute total

# Stable services
interval: 60s
count: 10       # 10 minutes total
```

---

## Next Steps

1. Review [DEPLOYMENT_STRATEGIES.md](./DEPLOYMENT_STRATEGIES.md) for detailed documentation
2. Customize analysis templates for your metrics
3. Configure Slack notifications
4. Set up custom dashboards in Grafana
5. Practice rollback procedures
6. Configure automated testing in CI/CD

---

## Support & Resources

- **Argo Rollouts Docs**: https://argoproj.github.io/argo-rollouts/
- **Prometheus Docs**: https://prometheus.io/docs/
- **Grafana Docs**: https://grafana.com/docs/
- **Kubectl Cheatsheet**: https://kubernetes.io/docs/reference/kubectl/cheatsheet/

---

**Last Updated**: 2025-11-27
