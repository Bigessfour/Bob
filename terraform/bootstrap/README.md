# Terraform Bootstrap — Remote State Backend

One-time setup that creates the S3 bucket and DynamoDB table for Terraform remote state.

## Prerequisites

- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) configured with credentials
- [Terraform](https://developer.hashicorp.com/terraform/install) >= 1.5

## Apply

```bash
cd terraform/bootstrap
terraform init
terraform plan
terraform apply
```

Save the outputs — you need them for the dev environment backend:

```bash
terraform output state_bucket_name
terraform output lock_table_name
terraform output aws_region
```

## Configure Dev Backend

After bootstrap, copy the backend example and fill in values from bootstrap outputs:

```bash
cd ../environments/dev
cp backend.tf.example backend.tf
# Edit backend.tf with bucket and dynamodb_table from bootstrap outputs
```

Then:

```bash
terraform init
terraform plan -var-file=terraform.tfvars
```

## Notes

- Bootstrap uses **local state** (stored in `terraform/bootstrap/terraform.tfstate`)
- Keep bootstrap state file safe — it tracks the state bucket itself
- `prevent_destroy` is set on the state bucket to avoid accidental deletion
