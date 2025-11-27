#!/bin/bash
# Setup Script for Advanced Deployment Infrastructure
# Installs: Argo Rollouts, AWS Load Balancer Controller, Nginx Ingress, Prometheus Operator

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "=========================================="
echo "Advanced Deployment Infrastructure Setup"
echo "=========================================="

# Configuration
EKS_CLUSTER_NAME=${EKS_CLUSTER_NAME:-"end-to-end-pipeline"}
AWS_REGION=${AWS_REGION:-"us-east-1"}
INSTALL_ARGO_ROLLOUTS=${INSTALL_ARGO_ROLLOUTS:-true}
INSTALL_AWS_LB_CONTROLLER=${INSTALL_AWS_LB_CONTROLLER:-true}
INSTALL_NGINX_INGRESS=${INSTALL_NGINX_INGRESS:-true}
INSTALL_PROMETHEUS=${INSTALL_PROMETHEUS:-true}

# Function to check prerequisites
check_prerequisites() {
    echo -e "${BLUE}Checking prerequisites...${NC}"

    # Check kubectl
    if ! command -v kubectl &> /dev/null; then
        echo -e "${RED}ERROR: kubectl not found${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ kubectl found${NC}"

    # Check helm
    if ! command -v helm &> /dev/null; then
        echo -e "${RED}ERROR: helm not found${NC}"
        echo "Install helm: https://helm.sh/docs/intro/install/"
        exit 1
    fi
    echo -e "${GREEN}✓ helm found${NC}"

    # Check cluster connection
    if ! kubectl cluster-info &> /dev/null; then
        echo -e "${RED}ERROR: Cannot connect to Kubernetes cluster${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ Connected to Kubernetes cluster${NC}"

    # Check AWS CLI (if installing AWS LB Controller)
    if [[ "$INSTALL_AWS_LB_CONTROLLER" == "true" ]]; then
        if ! command -v aws &> /dev/null; then
            echo -e "${YELLOW}WARNING: AWS CLI not found. Skipping AWS Load Balancer Controller.${NC}"
            INSTALL_AWS_LB_CONTROLLER=false
        else
            echo -e "${GREEN}✓ AWS CLI found${NC}"
        fi
    fi
}

# Function to install Argo Rollouts
install_argo_rollouts() {
    echo ""
    echo -e "${BLUE}Installing Argo Rollouts...${NC}"

    # Create namespace
    kubectl create namespace argo-rollouts --dry-run=client -o yaml | kubectl apply -f -

    # Add Argo Helm repository
    helm repo add argo https://argoproj.github.io/argo-helm
    helm repo update

    # Install Argo Rollouts
    helm upgrade --install argo-rollouts argo/argo-rollouts \
        --namespace argo-rollouts \
        --set dashboard.enabled=true \
        --set controller.metrics.enabled=true \
        --set controller.metrics.serviceMonitor.enabled=true \
        --wait

    echo -e "${GREEN}✓ Argo Rollouts installed${NC}"

    # Install kubectl plugin
    echo "Installing kubectl-argo-rollouts plugin..."
    curl -LO https://github.com/argoproj/argo-rollouts/releases/latest/download/kubectl-argo-rollouts-linux-amd64
    chmod +x kubectl-argo-rollouts-linux-amd64
    sudo mv kubectl-argo-rollouts-linux-amd64 /usr/local/bin/kubectl-argo-rollouts

    echo -e "${GREEN}✓ kubectl-argo-rollouts plugin installed${NC}"
}

# Function to install AWS Load Balancer Controller
install_aws_lb_controller() {
    echo ""
    echo -e "${BLUE}Installing AWS Load Balancer Controller...${NC}"

    # Download IAM policy
    curl -o iam_policy.json https://raw.githubusercontent.com/kubernetes-sigs/aws-load-balancer-controller/main/docs/install/iam_policy.json

    # Create IAM policy
    aws iam create-policy \
        --policy-name AWSLoadBalancerControllerIAMPolicy \
        --policy-document file://iam_policy.json \
        2>/dev/null || echo "Policy already exists, continuing..."

    # Get AWS account ID
    AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)

    # Create IAM service account
    eksctl create iamserviceaccount \
        --cluster="$EKS_CLUSTER_NAME" \
        --namespace=kube-system \
        --name=aws-load-balancer-controller \
        --attach-policy-arn=arn:aws:iam::"$AWS_ACCOUNT_ID":policy/AWSLoadBalancerControllerIAMPolicy \
        --override-existing-serviceaccounts \
        --approve

    # Add EKS Helm repository
    helm repo add eks https://aws.github.io/eks-charts
    helm repo update

    # Install AWS Load Balancer Controller
    helm upgrade --install aws-load-balancer-controller eks/aws-load-balancer-controller \
        --namespace kube-system \
        --set clusterName="$EKS_CLUSTER_NAME" \
        --set serviceAccount.create=false \
        --set serviceAccount.name=aws-load-balancer-controller \
        --set region="$AWS_REGION" \
        --set vpcId=$(aws eks describe-cluster --name "$EKS_CLUSTER_NAME" --query "cluster.resourcesVpcConfig.vpcId" --output text) \
        --wait

    echo -e "${GREEN}✓ AWS Load Balancer Controller installed${NC}"

    # Cleanup
    rm -f iam_policy.json
}

# Function to install Nginx Ingress Controller
install_nginx_ingress() {
    echo ""
    echo -e "${BLUE}Installing Nginx Ingress Controller...${NC}"

    # Add Nginx Helm repository
    helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
    helm repo update

    # Install Nginx Ingress
    helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx \
        --namespace ingress-nginx \
        --create-namespace \
        --set controller.metrics.enabled=true \
        --set controller.metrics.serviceMonitor.enabled=true \
        --set controller.podAnnotations."prometheus\.io/scrape"=true \
        --set controller.podAnnotations."prometheus\.io/port"=10254 \
        --wait

    echo -e "${GREEN}✓ Nginx Ingress Controller installed${NC}"
}

# Function to install Prometheus Operator
install_prometheus() {
    echo ""
    echo -e "${BLUE}Installing Prometheus Operator...${NC}"

    # Add Prometheus Helm repository
    helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
    helm repo update

    # Install kube-prometheus-stack
    helm upgrade --install kube-prometheus prometheus-community/kube-prometheus-stack \
        --namespace monitoring \
        --create-namespace \
        --set prometheus.prometheusSpec.serviceMonitorSelectorNilUsesHelmValues=false \
        --set prometheus.prometheusSpec.podMonitorSelectorNilUsesHelmValues=false \
        --set prometheus.prometheusSpec.ruleSelectorNilUsesHelmValues=false \
        --set grafana.enabled=true \
        --set grafana.adminPassword=admin \
        --set alertmanager.enabled=true \
        --wait

    echo -e "${GREEN}✓ Prometheus Operator installed${NC}"

    # Get Grafana password
    echo ""
    echo -e "${YELLOW}Grafana credentials:${NC}"
    echo "  Username: admin"
    echo "  Password: admin"
    echo ""
    echo "Access Grafana:"
    echo "  kubectl port-forward -n monitoring svc/kube-prometheus-grafana 3000:80"
    echo "  Then open: http://localhost:3000"
}

# Function to apply Kubernetes manifests
apply_manifests() {
    echo ""
    echo -e "${BLUE}Applying Kubernetes manifests...${NC}"

    MANIFESTS_DIR="../kubernetes"

    if [[ -f "$MANIFESTS_DIR/services.yaml" ]]; then
        kubectl apply -f "$MANIFESTS_DIR/services.yaml"
        echo -e "${GREEN}✓ Services applied${NC}"
    fi

    if [[ -f "$MANIFESTS_DIR/analysis-templates.yaml" ]]; then
        kubectl apply -f "$MANIFESTS_DIR/analysis-templates.yaml"
        echo -e "${GREEN}✓ Analysis templates applied${NC}"
    fi

    if [[ -f "$MANIFESTS_DIR/servicemonitors.yaml" ]]; then
        kubectl apply -f "$MANIFESTS_DIR/servicemonitors.yaml"
        echo -e "${GREEN}✓ ServiceMonitors applied${NC}"
    fi

    echo -e "${GREEN}✓ All manifests applied${NC}"
}

# Function to verify installation
verify_installation() {
    echo ""
    echo -e "${BLUE}Verifying installation...${NC}"

    # Check Argo Rollouts
    if [[ "$INSTALL_ARGO_ROLLOUTS" == "true" ]]; then
        if kubectl get pods -n argo-rollouts | grep -q Running; then
            echo -e "${GREEN}✓ Argo Rollouts is running${NC}"
        else
            echo -e "${RED}✗ Argo Rollouts is not running${NC}"
        fi
    fi

    # Check AWS Load Balancer Controller
    if [[ "$INSTALL_AWS_LB_CONTROLLER" == "true" ]]; then
        if kubectl get pods -n kube-system | grep aws-load-balancer-controller | grep -q Running; then
            echo -e "${GREEN}✓ AWS Load Balancer Controller is running${NC}"
        else
            echo -e "${RED}✗ AWS Load Balancer Controller is not running${NC}"
        fi
    fi

    # Check Nginx Ingress
    if [[ "$INSTALL_NGINX_INGRESS" == "true" ]]; then
        if kubectl get pods -n ingress-nginx | grep -q Running; then
            echo -e "${GREEN}✓ Nginx Ingress is running${NC}"
        else
            echo -e "${RED}✗ Nginx Ingress is not running${NC}"
        fi
    fi

    # Check Prometheus
    if [[ "$INSTALL_PROMETHEUS" == "true" ]]; then
        if kubectl get pods -n monitoring | grep prometheus | grep -q Running; then
            echo -e "${GREEN}✓ Prometheus is running${NC}"
        else
            echo -e "${RED}✗ Prometheus is not running${NC}"
        fi
    fi
}

# Function to print next steps
print_next_steps() {
    echo ""
    echo "=========================================="
    echo -e "${GREEN}Installation Complete!${NC}"
    echo "=========================================="
    echo ""
    echo "Next Steps:"
    echo ""
    echo "1. Deploy using Blue/Green strategy:"
    echo "   ./deploy-blue-green.sh airflow v1.0.0"
    echo ""
    echo "2. Deploy using Canary strategy:"
    echo "   ./deploy-canary.sh airflow v1.0.0"
    echo ""
    echo "3. Access Argo Rollouts dashboard:"
    echo "   kubectl port-forward -n argo-rollouts svc/argo-rollouts-dashboard 3100:3100"
    echo "   Then open: http://localhost:3100"
    echo ""
    echo "4. Access Grafana dashboard:"
    echo "   kubectl port-forward -n monitoring svc/kube-prometheus-grafana 3000:80"
    echo "   Then open: http://localhost:3000 (admin/admin)"
    echo ""
    echo "5. View rollouts:"
    echo "   kubectl argo rollouts list"
    echo ""
    echo "6. Apply rollout manifests:"
    echo "   kubectl apply -f ../kubernetes/rollout-blue-green.yaml"
    echo "   kubectl apply -f ../kubernetes/rollout-canary.yaml"
    echo ""
}

# Main execution
main() {
    check_prerequisites

    if [[ "$INSTALL_ARGO_ROLLOUTS" == "true" ]]; then
        install_argo_rollouts
    fi

    if [[ "$INSTALL_AWS_LB_CONTROLLER" == "true" ]]; then
        install_aws_lb_controller
    fi

    if [[ "$INSTALL_NGINX_INGRESS" == "true" ]]; then
        install_nginx_ingress
    fi

    if [[ "$INSTALL_PROMETHEUS" == "true" ]]; then
        install_prometheus
    fi

    apply_manifests
    verify_installation
    print_next_steps
}

# Run main function
main
