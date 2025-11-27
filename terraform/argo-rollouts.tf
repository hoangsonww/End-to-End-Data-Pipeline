# Argo Rollouts Infrastructure
# Enables advanced deployment strategies: Blue/Green, Canary, Progressive Delivery

# Namespace for Argo Rollouts
resource "kubernetes_namespace" "argo_rollouts" {
  metadata {
    name = "argo-rollouts"

    labels = {
      name    = "argo-rollouts"
      purpose = "progressive-delivery"
    }
  }
}

# Helm release for Argo Rollouts
resource "helm_release" "argo_rollouts" {
  name       = "argo-rollouts"
  repository = "https://argoproj.github.io/argo-helm"
  chart      = "argo-rollouts"
  namespace  = kubernetes_namespace.argo_rollouts.metadata[0].name
  version    = "2.32.0"

  values = [
    yamlencode({
      # Controller configuration
      controller = {
        replicas = 2

        resources = {
          limits = {
            cpu    = "500m"
            memory = "512Mi"
          }
          requests = {
            cpu    = "250m"
            memory = "256Mi"
          }
        }

        # Enable metrics
        metrics = {
          enabled      = true
          serviceMonitor = {
            enabled = true
          }
        }

        # Enable notifications
        notifications = {
          enabled = true
          secret = {
            create = true
            items = {
              slack-token = ""  # Add your Slack token here or use external secret
            }
          }
        }
      }

      # Dashboard configuration
      dashboard = {
        enabled = true

        service = {
          type = "LoadBalancer"
          port = 3100
        }

        ingress = {
          enabled = false  # Enable if you want ingress
          annotations = {
            "kubernetes.io/ingress.class"                = "nginx"
            "cert-manager.io/cluster-issuer"             = "letsencrypt-prod"
            "nginx.ingress.kubernetes.io/ssl-redirect"   = "true"
          }
          hosts = [
            {
              host = "rollouts.yourdomain.com"
              paths = [
                {
                  path     = "/"
                  pathType = "Prefix"
                }
              ]
            }
          ]
        }
      }

      # Service account
      serviceAccount = {
        create = true
        name   = "argo-rollouts"
      }
    })
  ]

  depends_on = [
    kubernetes_namespace.argo_rollouts
  ]
}

# Install Argo Rollouts kubectl plugin via init container approach
resource "kubernetes_config_map" "rollouts_plugin_installer" {
  metadata {
    name      = "rollouts-plugin-installer"
    namespace = kubernetes_namespace.argo_rollouts.metadata[0].name
  }

  data = {
    "install.sh" = <<-EOT
      #!/bin/bash
      set -e

      # Download kubectl-argo-rollouts plugin
      curl -LO https://github.com/argoproj/argo-rollouts/releases/latest/download/kubectl-argo-rollouts-linux-amd64
      chmod +x kubectl-argo-rollouts-linux-amd64
      mv kubectl-argo-rollouts-linux-amd64 /usr/local/bin/kubectl-argo-rollouts

      echo "Argo Rollouts plugin installed successfully"
    EOT
  }
}

# RBAC for Argo Rollouts to manage deployments
resource "kubernetes_cluster_role" "argo_rollouts_aggregate" {
  metadata {
    name = "argo-rollouts-aggregate-to-admin"

    labels = {
      "rbac.authorization.k8s.io/aggregate-to-admin" = "true"
      "rbac.authorization.k8s.io/aggregate-to-edit"  = "true"
    }
  }

  rule {
    api_groups = ["argoproj.io"]
    resources  = ["rollouts", "rollouts/status", "experiments", "analysisruns", "analysistemplates"]
    verbs      = ["get", "list", "watch", "create", "update", "patch", "delete"]
  }
}

# Service Monitor for Prometheus (if using Prometheus Operator)
resource "kubernetes_manifest" "argo_rollouts_servicemonitor" {
  manifest = {
    apiVersion = "monitoring.coreos.com/v1"
    kind       = "ServiceMonitor"

    metadata = {
      name      = "argo-rollouts-metrics"
      namespace = kubernetes_namespace.argo_rollouts.metadata[0].name

      labels = {
        app                          = "argo-rollouts"
        "app.kubernetes.io/name"     = "argo-rollouts-metrics"
        "app.kubernetes.io/instance" = "argo-rollouts"
      }
    }

    spec = {
      selector = {
        matchLabels = {
          "app.kubernetes.io/name" = "argo-rollouts-metrics"
        }
      }

      endpoints = [
        {
          port     = "metrics"
          interval = "30s"
          path     = "/metrics"
        }
      ]
    }
  }

  depends_on = [helm_release.argo_rollouts]
}

# Notification Secret for Argo Rollouts
resource "kubernetes_secret" "argo_rollouts_notification" {
  metadata {
    name      = "argo-rollouts-notification-secret"
    namespace = kubernetes_namespace.argo_rollouts.metadata[0].name
  }

  data = {
    slack-token = base64encode(var.slack_webhook_url)
  }

  type = "Opaque"
}

# ConfigMap for Argo Rollouts notifications
resource "kubernetes_config_map" "argo_rollouts_notification_config" {
  metadata {
    name      = "argo-rollouts-notification-configmap"
    namespace = kubernetes_namespace.argo_rollouts.metadata[0].name
  }

  data = {
    "service.slack" = <<-EOT
      token: $slack-token
    EOT

    "template.rollout-updated" = <<-EOT
      message: |
        Rollout {{.rollout.metadata.name}} has been updated.
        Namespace: {{.rollout.metadata.namespace}}
        Phase: {{.rollout.status.phase}}
    EOT

    "template.rollout-step-completed" = <<-EOT
      message: |
        ✅ Rollout {{.rollout.metadata.name}} completed step {{.rollout.status.currentStepIndex}}.
        Phase: {{.rollout.status.phase}}
    EOT

    "template.rollout-aborted" = <<-EOT
      message: |
        ⚠️ Rollout {{.rollout.metadata.name}} has been ABORTED!
        Namespace: {{.rollout.metadata.namespace}}
        Message: {{.rollout.status.message}}
    EOT

    "template.rollout-completed" = <<-EOT
      message: |
        🎉 Rollout {{.rollout.metadata.name}} completed successfully!
        Namespace: {{.rollout.metadata.namespace}}
    EOT

    "template.analysis-run-failed" = <<-EOT
      message: |
        ❌ Analysis Run {{.analysisRun.metadata.name}} FAILED!
        Rollout: {{.analysisRun.metadata.labels.rollout}}
        Message: {{.analysisRun.status.message}}
    EOT

    "trigger.on-rollout-updated" = <<-EOT
      - send: [rollout-updated]
    EOT

    "trigger.on-rollout-step-completed" = <<-EOT
      - send: [rollout-step-completed]
    EOT

    "trigger.on-rollout-completed" = <<-EOT
      - when: rollout.status.phase == 'Healthy'
        send: [rollout-completed]
    EOT

    "trigger.on-rollout-aborted" = <<-EOT
      - when: rollout.status.phase == 'Degraded'
        send: [rollout-aborted]
    EOT

    "trigger.on-analysis-run-failed" = <<-EOT
      - when: analysisRun.status.phase == 'Failed'
        send: [analysis-run-failed]
    EOT
  }
}

# Variables
variable "slack_webhook_url" {
  description = "Slack webhook URL for Argo Rollouts notifications"
  type        = string
  default     = ""
  sensitive   = true
}

# Outputs
output "argo_rollouts_namespace" {
  description = "Namespace where Argo Rollouts is installed"
  value       = kubernetes_namespace.argo_rollouts.metadata[0].name
}

output "argo_rollouts_dashboard_url" {
  description = "URL for Argo Rollouts dashboard (if LoadBalancer)"
  value       = "Check kubectl get svc -n ${kubernetes_namespace.argo_rollouts.metadata[0].name} for LoadBalancer IP"
}
