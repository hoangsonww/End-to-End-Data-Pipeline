#!/bin/bash
# Blue/Green Deployment Script with Preview and Promotion
# Usage: ./deploy-blue-green.sh <service-name> <new-image-tag>

set -e

SERVICE_NAME=${1:-airflow}
IMAGE_TAG=${2:-latest}
NAMESPACE=${3:-default}

echo "=========================================="
echo "Starting Blue/Green Deployment"
echo "Service: $SERVICE_NAME"
echo "Image Tag: $IMAGE_TAG"
echo "Namespace: $NAMESPACE"
echo "=========================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to check if Argo Rollouts is installed
check_argo_rollouts() {
    if ! kubectl get crd rollouts.argoproj.io &> /dev/null; then
        echo -e "${RED}ERROR: Argo Rollouts is not installed!${NC}"
        echo "Please install Argo Rollouts first"
        exit 1
    fi
    echo -e "${GREEN}✓ Argo Rollouts is installed${NC}"
}

# Function to check if kubectl-argo-rollouts plugin is available
check_rollouts_plugin() {
    if ! kubectl argo rollouts version &> /dev/null; then
        echo -e "${YELLOW}WARNING: kubectl-argo-rollouts plugin not found${NC}"
        USE_PLUGIN=false
    else
        echo -e "${GREEN}✓ kubectl-argo-rollouts plugin found${NC}"
        USE_PLUGIN=true
    fi
}

# Function to get current active version
get_active_version() {
    local rollout_name="${SERVICE_NAME}-rollout"

    echo -e "${BLUE}Current Active Version (Blue):${NC}"
    kubectl get rollout "$rollout_name" -n "$NAMESPACE" \
        -o jsonpath='{.status.stableRS}' 2>/dev/null || echo "Unknown"
}

# Function to update rollout with new image (Green)
update_rollout() {
    local rollout_name="${SERVICE_NAME}-rollout"

    echo -e "${YELLOW}Deploying new version (Green)...${NC}"

    if [[ "$USE_PLUGIN" == "true" ]]; then
        kubectl argo rollouts set image "$rollout_name" \
            "*=myrepo/${SERVICE_NAME}-pipeline:${IMAGE_TAG}" \
            -n "$NAMESPACE"
    else
        kubectl set image rollout/"$rollout_name" \
            "*=myrepo/${SERVICE_NAME}-pipeline:${IMAGE_TAG}" \
            -n "$NAMESPACE"
    fi

    echo -e "${GREEN}✓ New version deployed to preview environment${NC}"
}

# Function to get preview service endpoint
get_preview_endpoint() {
    local preview_service="${SERVICE_NAME}-webserver-preview"

    echo -e "${BLUE}Preview Service Endpoint:${NC}"

    # Get service type
    SERVICE_TYPE=$(kubectl get svc "$preview_service" -n "$NAMESPACE" -o jsonpath='{.spec.type}' 2>/dev/null || echo "")

    if [[ "$SERVICE_TYPE" == "LoadBalancer" ]]; then
        ENDPOINT=$(kubectl get svc "$preview_service" -n "$NAMESPACE" \
            -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>/dev/null || \
            kubectl get svc "$preview_service" -n "$NAMESPACE" \
            -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "Pending")
        echo "http://${ENDPOINT}:8080"
    elif [[ "$SERVICE_TYPE" == "ClusterIP" ]]; then
        echo "ClusterIP service - use port-forward:"
        echo "  kubectl port-forward -n $NAMESPACE svc/$preview_service 8081:8080"
        echo "  Then access: http://localhost:8081"
    else
        echo -e "${YELLOW}Service not found or not ready${NC}"
    fi
}

# Function to run preview analysis
run_preview_analysis() {
    local rollout_name="${SERVICE_NAME}-rollout"

    echo -e "${YELLOW}Running preview analysis...${NC}"

    # Wait for preview pods to be ready
    echo "Waiting for preview pods to be ready..."
    sleep 10

    # Check if analysis templates exist
    if kubectl get analysistemplate -n "$NAMESPACE" &> /dev/null; then
        echo "Analysis templates found, checking results..."

        # Get latest analysis run
        LATEST_ANALYSIS=$(kubectl get analysisrun -n "$NAMESPACE" \
            --sort-by=.metadata.creationTimestamp \
            -o jsonpath='{.items[-1].metadata.name}' 2>/dev/null || echo "")

        if [[ -n "$LATEST_ANALYSIS" ]]; then
            echo "Latest analysis run: $LATEST_ANALYSIS"
            kubectl get analysisrun "$LATEST_ANALYSIS" -n "$NAMESPACE" -o wide
        else
            echo -e "${YELLOW}No analysis runs found${NC}"
        fi
    else
        echo -e "${YELLOW}No analysis templates configured${NC}"
        echo "Skipping automated analysis. Please test manually."
    fi
}

# Function to test preview environment
test_preview() {
    local preview_service="${SERVICE_NAME}-webserver-preview"

    echo -e "${YELLOW}Testing preview environment...${NC}"

    # Try to get service endpoint
    if kubectl get svc "$preview_service" -n "$NAMESPACE" &> /dev/null; then
        echo "Preview service exists. Testing health endpoint..."

        # Port forward for testing
        echo "Setting up port-forward for testing..."
        kubectl port-forward -n "$NAMESPACE" svc/"$preview_service" 8081:8080 &
        PF_PID=$!
        sleep 5

        # Test health endpoint
        if curl -sf http://localhost:8081/health &> /dev/null; then
            echo -e "${GREEN}✓ Health check passed${NC}"
        else
            echo -e "${RED}✗ Health check failed${NC}"
        fi

        # Clean up port-forward
        kill $PF_PID 2>/dev/null || true
    else
        echo -e "${YELLOW}Preview service not found${NC}"
    fi
}

# Function to promote green to blue
promote_to_production() {
    local rollout_name="${SERVICE_NAME}-rollout"

    echo -e "${BLUE}Current deployment status:${NC}"
    get_rollout_status

    echo ""
    echo -e "${YELLOW}This will promote the Green (preview) environment to Blue (production).${NC}"
    echo -e "${YELLOW}Are you sure? (yes/no)${NC}"
    read -r response

    if [[ "$response" != "yes" ]]; then
        echo "Promotion cancelled."
        return
    fi

    echo -e "${YELLOW}Promoting Green to Blue...${NC}"

    if [[ "$USE_PLUGIN" == "true" ]]; then
        kubectl argo rollouts promote "$rollout_name" -n "$NAMESPACE"
    else
        # Manually promote by updating the rollout
        kubectl patch rollout "$rollout_name" -n "$NAMESPACE" \
            --type json -p '[{"op": "replace", "path": "/spec/strategy/blueGreen/autoPromotionEnabled", "value": true}]'
    fi

    echo -e "${GREEN}✓ Promotion initiated${NC}"
    echo "Waiting for rollout to complete..."

    # Wait for rollout
    kubectl rollout status rollout/"$rollout_name" -n "$NAMESPACE" --timeout=10m

    echo -e "${GREEN}✓ Rollout completed successfully!${NC}"
}

# Function to rollback to blue
rollback_to_blue() {
    local rollout_name="${SERVICE_NAME}-rollout"

    echo -e "${RED}Rolling back to Blue (stable) version...${NC}"

    if [[ "$USE_PLUGIN" == "true" ]]; then
        kubectl argo rollouts abort "$rollout_name" -n "$NAMESPACE"
        kubectl argo rollouts undo "$rollout_name" -n "$NAMESPACE"
    else
        kubectl rollout undo rollout/"$rollout_name" -n "$NAMESPACE"
    fi

    echo -e "${GREEN}✓ Rollback initiated${NC}"
}

# Function to get rollout status
get_rollout_status() {
    local rollout_name="${SERVICE_NAME}-rollout"

    if [[ "$USE_PLUGIN" == "true" ]]; then
        kubectl argo rollouts get rollout "$rollout_name" -n "$NAMESPACE"
    else
        kubectl get rollout "$rollout_name" -n "$NAMESPACE" -o wide
        echo ""
        echo "Pods:"
        kubectl get pods -n "$NAMESPACE" -l app=pipeline,component="$SERVICE_NAME"
    fi
}

# Function to watch rollout
watch_rollout() {
    local rollout_name="${SERVICE_NAME}-rollout"

    echo -e "${YELLOW}Watching rollout progress...${NC}"

    if [[ "$USE_PLUGIN" == "true" ]]; then
        kubectl argo rollouts get rollout "$rollout_name" -n "$NAMESPACE" --watch
    else
        watch kubectl get rollout "$rollout_name" -n "$NAMESPACE"
    fi
}

# Function to compare Blue vs Green metrics
compare_metrics() {
    echo -e "${BLUE}Comparing Blue vs Green Metrics:${NC}"

    # Active service metrics
    echo -e "${BLUE}Blue (Active) Metrics:${NC}"
    kubectl top pods -n "$NAMESPACE" -l app=pipeline,component="$SERVICE_NAME" | grep -v NAME | head -3 || echo "Metrics not available"

    echo ""
    echo -e "${GREEN}Green (Preview) Metrics:${NC}"
    kubectl top pods -n "$NAMESPACE" -l app=pipeline,component="$SERVICE_NAME" | grep -v NAME | tail -3 || echo "Metrics not available"
}

# Main menu
show_menu() {
    echo ""
    echo "=========================================="
    echo "Blue/Green Deployment Menu"
    echo "=========================================="
    echo "  1) Get current status"
    echo "  2) View preview endpoint"
    echo "  3) Test preview environment"
    echo "  4) Run preview analysis"
    echo "  5) Compare Blue vs Green metrics"
    echo "  6) Promote Green to Blue (Production)"
    echo "  7) Rollback to Blue"
    echo "  8) Watch rollout progress"
    echo "  9) Exit"
    echo "=========================================="
}

# Main execution
main() {
    echo "Pre-flight checks..."
    check_argo_rollouts
    check_rollouts_plugin

    echo ""
    get_active_version

    echo ""
    echo "Deploying new version..."
    update_rollout

    echo ""
    get_preview_endpoint

    # Interactive menu
    while true; do
        show_menu
        read -rp "Choose an option (1-9): " option

        case $option in
            1)
                get_rollout_status
                ;;
            2)
                get_preview_endpoint
                ;;
            3)
                test_preview
                ;;
            4)
                run_preview_analysis
                ;;
            5)
                compare_metrics
                ;;
            6)
                promote_to_production
                ;;
            7)
                rollback_to_blue
                ;;
            8)
                watch_rollout
                ;;
            9)
                echo "Exiting. Preview environment will remain active."
                echo "Use option 6 to promote or option 7 to rollback."
                break
                ;;
            *)
                echo -e "${RED}Invalid option${NC}"
                ;;
        esac
    done
}

# Run main function
main
