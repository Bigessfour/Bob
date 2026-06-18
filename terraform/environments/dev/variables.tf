variable "project_name" {
  description = "Project name used for resource naming"
  type        = string
  default     = "bob"
}

variable "aws_region" {
  description = "AWS region for resources"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Deployment environment"
  type        = string
  default     = "dev"
}
