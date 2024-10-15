# Makefile for Terraform Azure Infrastructure

# Variables
TERRAFORM_DIR = ./terraform
TERRAFORM = terraform

# Phony targets
.PHONY: init plan apply destroy clean

# Initialize Terraform
init:
	cd $(TERRAFORM_DIR) && $(TERRAFORM) init

# Plan Terraform changes
plan:
	cd $(TERRAFORM_DIR) && $(TERRAFORM) plan

# Apply Terraform changes
apply:
	cd $(TERRAFORM_DIR) && $(TERRAFORM) apply -auto-approve

# Destroy Terraform-managed infrastructure
destroy:
	cd $(TERRAFORM_DIR) && $(TERRAFORM) destroy -auto-approve

# Clean up Terraform files
clean:
	cd $(TERRAFORM_DIR) && rm -rf .terraform *.tfstate* *.tfplan

# Show Terraform outputs
output:
	cd $(TERRAFORM_DIR) && $(TERRAFORM) output

# Validate Terraform files
validate:
	cd $(TERRAFORM_DIR) && $(TERRAFORM) validate

# Format Terraform files
fmt:
	cd $(TERRAFORM_DIR) && $(TERRAFORM) fmt -recursive

# Default target
all: init plan

# Help target
help:
	@echo "Available targets:"
	@echo "  init      : Initialize Terraform"
	@echo "  plan      : Plan Terraform changes"
	@echo "  apply     : Apply Terraform changes"
	@echo "  destroy   : Destroy Terraform-managed infrastructure"
	@echo "  clean     : Clean up Terraform files"
	@echo "  output    : Show Terraform outputs"
	@echo "  validate  : Validate Terraform files"
	@echo "  fmt       : Format Terraform files"
	@echo "  all       : Run init and plan"
	@echo "  help      : Show this help message"