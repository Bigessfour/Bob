output "site_bucket_name" {
  description = "S3 bucket for WebGL build artifacts"
  value       = aws_s3_bucket.site.bucket
}

output "cloudfront_distribution_id" {
  description = "CloudFront distribution ID (for cache invalidation)"
  value       = aws_cloudfront_distribution.site.id
}

output "cloudfront_domain_name" {
  description = "CloudFront URL for the WebGL demo"
  value       = aws_cloudfront_distribution.site.domain_name
}

output "cloudfront_url" {
  description = "HTTPS URL for the live demo"
  value       = "https://${aws_cloudfront_distribution.site.domain_name}"
}
