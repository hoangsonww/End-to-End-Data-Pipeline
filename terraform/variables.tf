variable "aws_region" {
  description = "AWS region to deploy resources"
  default     = "us-east-1"
}

variable "instance_type" {
  description = "EC2 instance type"
  default     = "t3.medium"
}

variable "db_username" {
  description = "Database username"
  default     = "admin"
}

variable "db_password" {
  description = "Database password"
}

variable "eks_cluster_name" {
  description = "EKS Cluster Name"
  default     = "end-to-end-pipeline"
}

variable "slack_webhook_url" {
  description = "Slack webhook URL for deployment notifications"
  type        = string
  default     = ""
  sensitive   = true
}

variable "enable_blue_green" {
  description = "Enable blue/green deployment strategy"
  type        = bool
  default     = true
}

variable "enable_canary" {
  description = "Enable canary deployment strategy"
  type        = bool
  default     = true
}

variable "canary_initial_weight" {
  description = "Initial traffic weight for canary deployments (percentage)"
  type        = number
  default     = 10
}

variable "enable_monitoring" {
  description = "Enable Prometheus and Grafana monitoring"
  type        = bool
  default     = true
}
