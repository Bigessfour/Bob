# Terraform — Bob Portfolio Site Hosting

Infrastructure as Code for hosting the Bob **portfolio static site** on AWS Free Tier (S3 + CloudFront). HDRP does not support WebGL — the live demo is a static landing page (progress gallery, GIFs, write-up), not an in-browser Unity build.

## Architecture

```text
terraform/
├── bootstrap/              # One-time: S3 state bucket + DynamoDB lock table
└── environments/
    └── dev/                # S3 site bucket + CloudFront distribution
```

## Prerequisites

- AWS CLI configured (`aws configure`)
- Terraform >= 1.5
- IAM permissions for S3, CloudFront, DynamoDB

## Apply Order

### 1. Bootstrap (one-time)

Creates remote state backend. Uses local state for itself.

```bash
cd terraform/bootstrap
terraform init
terraform apply
```

Note the outputs: `state_bucket_name`, `lock_table_name`, `aws_region`.

### 2. Dev Environment

Configure remote state backend, then deploy hosting stack.

```bash
cd terraform/environments/dev
cp backend.tf.example backend.tf
# Edit backend.tf — replace REPLACE_WITH_* placeholders with bootstrap outputs

cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars if needed

terraform init
terraform plan
terraform apply
```

After apply, note `cloudfront_url` output — this becomes the live portfolio link.

### 3. Deploy Portfolio Site (Week 3)

```bash
# Static HTML/assets under docs/portfolio-site/ (GIFs, gallery, write-up links)
aws s3 sync docs/portfolio-site/ s3://$(terraform output -raw site_bucket_name)/ \
  --delete \
  --cache-control "public, max-age=3600"

aws cloudfront create-invalidation \
  --distribution-id $(terraform output -raw cloudfront_distribution_id) \
  --paths "/*"
```

## CI Validation

GitHub Actions runs `terraform fmt -check` and `terraform validate` on every push. No AWS credentials required for validation (`terraform init -backend=false`).

## Cost

Designed for AWS Free Tier:

- S3 storage for static portfolio files
- CloudFront data transfer (1 TB/month free first 12 months)
- DynamoDB on-demand for state locking (minimal usage)

## Security

- S3 buckets block public access; CloudFront OAC serves content over HTTPS
- Never commit `terraform.tfvars` or `backend.tf` with real bucket names if sensitive
- `backend.tf` is gitignored if you prefer — use `backend.tf.example` as template
