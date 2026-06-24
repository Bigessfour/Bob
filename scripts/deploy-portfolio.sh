#!/usr/bin/env bash
# Sync docs/portfolio-site/ to the Terraform dev S3 bucket and invalidate CloudFront.
#
# Policy: Do NOT run against the AICO AWS account. Use a dedicated portfolio profile, e.g.:
#   export AWS_PROFILE=your-portfolio-profile
#   aws sts get-caller-identity   # confirm account before apply/sync
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
TF_DIR="${REPO_ROOT}/terraform/environments/dev"

if [[ ! -f ${TF_DIR}/backend.tf ]]; then
	echo "Missing ${TF_DIR}/backend.tf — copy backend.tf.example and apply bootstrap first." >&2
	echo "See terraform/README.md" >&2
	exit 1
fi

cd "${TF_DIR}"
BUCKET="$(terraform output -raw site_bucket_name)"
DIST_ID="$(terraform output -raw cloudfront_distribution_id)"
CF_URL="$(terraform output -raw cloudfront_url)"

echo "Syncing portfolio site to s3://${BUCKET} ..."
aws s3 sync "${REPO_ROOT}/docs/portfolio-site/" "s3://${BUCKET}/" \
	--delete \
	--cache-control "public, max-age=3600"

echo "Invalidating CloudFront distribution ${DIST_ID} ..."
aws cloudfront create-invalidation \
	--distribution-id "${DIST_ID}" \
	--paths "/*" \
	--output text

echo "DEPLOY_OK: ${CF_URL}"
