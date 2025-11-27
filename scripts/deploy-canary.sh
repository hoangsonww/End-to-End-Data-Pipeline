#!/bin/bash
# Canary Deployment Script with Progressive Traffic Shifting
# Usage: ./deploy-canary.sh <service-name> <new-image-tag>

set -e

SERVICE_NAME=${1:-airflow}
IMAGE_TAG=${2:-latest}
NAMESPACE=${3:-default}

echo "=========================================="
echo "Starting Canary Deployment"
echo "Service: $SERVICE_NAME"
echo "Image Tag: $IMAGE_TAG"
echo "Namespace: $NAMESPACE"
echo "=========================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check if Argo Rollouts is installed
check_argo_rollouts() {
    if ! kubectl get crd rollouts.argoproj.io &> /dev/null; then
        echo -e "${RED}ERROR: Argo Rollouts is not installed!${NC}"
        echo "Please install Argo Rollouts first:"
        echo "  kubectl create namespace argo-rollouts"
        echo "  kubectl apply -n argo-rollouts -f https://github.com/argoproj/argo-rollouts/releases/latest/download/install.yaml"
        exit 1
    fi
    echo -e "${GREEN}✓ Argo Rollouts is installed${NC}"
}

# Function to check if kubectl-argo-rollouts plugin is available
check_rollouts_plugin() {
    if ! kubectl argo rollouts version &> /dev/null; then
        echo -e "${YELLOW}WARNING: kubectl-argo-rollouts plugin not found${NC}"
        echo "Install with: curl -LO https://github.com/argoproj/argo-rollouts/releases/latest/download/kubectl-argo-rollouts-linux-amd64"
        echo "Using kubectl commands instead..."
        USE_PLUGIN=false
    else
        echo -e "${GREEN}✓ kubectl-argo-rollouts plugin found${NC}"
        USE_PLUGIN=true
    fi
}

# Function to update rollout with new image
update_rollout() {
    local rollout_name="${SERVICE_NAME}-canary-rollout"

    echo -e "${YELLOW}Updating rollout ${rollout_name} with new image...${NC}"

    if [[ "$USE_PLUGIN" == "true" ]]; then
        kubectl argo rollouts set image "$rollout_name" \
            "*=myrepo/${SERVICE_NAME}-pipeline:${IMAGE_TAG}" \
            -n "$NAMESPACE"
    else
        kubectl set image rollout/"$rollout_name" \
            "*=myrepo/${SERVICE_NAME}-pipeline:${IMAGE_TAG}" \
            -n "$NAMESPACE"
    fi

    echo -e "${GREEN}✓ Rollout updated with new image${NC}"
}

# Function to watch rollout status
watch_rollout() {
    local rollout_name="${SERVICE_NAME}-canary-rollout"

    echo -e "${YELLOW}Watching rollout progress...${NC}"
    echo "Press Ctrl+C to stop watching (rollout will continue in background)"

    if [[ "$USE_PLUGIN" == "true" ]]; then
        kubectl argo rollouts get rollout "$rollout_name" -n "$NAMESPACE" --watch
    else
        kubectl rollout status rollout/"$rollout_name" -n "$NAMESPACE" --watch
    fi
}

# Function to promote canary
promote_canary() {
    local rollout_name="${SERVICE_NAME}-canary-rollout"

    echo -e "${YELLOW}Do you want to promote the canary to stable? (y/n)${NC}"
    read -r response

    if [[ "$response" == "y" ]]; then
        echo -e "${YELLOW}Promoting canary...${NC}"

        if [[ "$USE_PLUGIN" == "true" ]]; then
            kubectl argo rollouts promote "$rollout_name" -n "$NAMESPACE"
        else
            kubectl patch rollout "$rollout_name" -n "$NAMESPACE" \
                --type json -p '[{"op": "replace", "path": "/spec/strategy/canary/steps", "value": [{"setWeight": 100}]}]'
        fi

        echo -e "${GREEN}✓ Canary promoted to stable${NC}"
    else
        echo -e "${YELLOW}Promotion cancelled. Canary will continue at current weight.${NC}"
    fi
}

# Function to abort rollout
abort_rollout() {
    local rollout_name="${SERVICE_NAME}-canary-rollout"

    echo -e "${RED}Aborting rollout...${NC}"

    if [[ "$USE_PLUGIN" == "true" ]]; then
        kubectl argo rollouts abort "$rollout_name" -n "$NAMESPACE"
    else
        kubectl patch rollout "$rollout_name" -n "$NAMESPACE" \
            --type merge -p '{"spec":{"paused":true}}'
    fi

    echo -e "${GREEN}✓ Rollout aborted${NC}"
}

# Function to get rollout status
get_rollout_status() {
    local rollout_name="${SERVICE_NAME}-canary-rollout"

    echo -e "${YELLOW}Current Rollout Status:${NC}"

    if [[ "$USE_PLUGIN" == "true" ]]; then
        kubectl argo rollouts get rollout "$rollout_name" -n "$NAMESPACE"
    else
        kubectl get rollout "$rollout_name" -n "$NAMESPACE" -o wide
    fi
}

# Function to get analysis runs
get_analysis_runs() {
    echo -e "${YELLOW}Recent Analysis Runs:${NC}"
    kubectl get analysisruns -n "$NAMESPACE" --sort-by=.metadata.creationTimestamp | tail -10
}

# Function to check metrics
check_metrics() {
    echo -e "${YELLOW}Checking canary metrics...${NC}"

    # Check if Prometheus is available
    if kubectl get svc prometheus -n "$NAMESPACE" &> /dev/null; then
        echo "Success Rate:"
        kubectl exec -n "$NAMESPACE" svc/prometheus -- \
            wget -qO- 'http://localhost:9090/api/v1/query?query=sum(rate(http_requests_total{service=~".*-canary",status=~"2.."}[5m]))/sum(rate(http_requests_total{service=~".*-canary"}[5m]))' \
            | jq '.data.result[0].value[1]' 2>/dev/null || echo "Unable to fetch metrics"
    else
        echo -e "${YELLOW}Prometheus not available in namespace${NC}"
    fi
}

# Main execution
main() {
    echo "Pre-flight checks..."
    check_argo_rollouts
    check_rollouts_plugin

    echo ""
    echo "Starting deployment..."
    update_rollout

    echo ""
    get_rollout_status

    echo ""
    echo "Options:"
    echo "  1) Watch rollout progress"
    echo "  2) Promote canary"
    echo "  3) Abort rollout"
    echo "  4) Check metrics"
    echo "  5) View analysis runs"
    echo "  6) Exit"

    while true; do
        echo ""
        read -rp "Choose an option (1-6): " option

        case $option in
            1)
                watch_rollout
                ;;
            2)
                promote_canary
                ;;
            3)
                abort_rollout
                break
                ;;
            4)
                check_metrics
                ;;
            5)
                get_analysis_runs
                ;;
            6)
                echo "Exiting. Rollout will continue in background."
                echo "Use 'kubectl argo rollouts get rollout ${SERVICE_NAME}-canary-rollout -n ${NAMESPACE}' to check status"
                break
                ;;
            *)
                echo "Invalid option"
                ;;
        esac
    done
}

# Run main function
main
