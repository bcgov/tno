#!/bin/bash
# Compare deployed Deployment with Production to find discrepancies

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check arguments
if [ $# -lt 2 ]; then
    echo "Usage: $0 <service-name> <dev|test|prod>"
    echo "Example: $0 nginx dev"
    exit 1
fi

SERVICE=$1
ENV=$2

# Set namespaces
case $ENV in
    dev)
        NAMESPACE="9b301c-dev"
        ;;
    test)
        NAMESPACE="9b301c-test"
        ;;
    prod)
        NAMESPACE="9b301c-prod"
        ;;
    *)
        echo "Invalid environment. Use: dev, test, or prod"
        exit 1
        ;;
esac

PROD_NAMESPACE="9b301c-prod"
TEMP_DIR="/tmp/deployment-compare-$$"
mkdir -p "$TEMP_DIR"

echo -e "${GREEN}Comparing $SERVICE deployment in $ENV vs Production${NC}"
echo "=================================================="

# Function to extract key fields from YAML
extract_key_fields() {
    local file=$1
    local output=$2

    # Extract only the important fields, ignore runtime metadata
    yq eval '
        del(.metadata.generation) |
        del(.metadata.resourceVersion) |
        del(.metadata.uid) |
        del(.metadata.creationTimestamp) |
        del(.metadata.managedFields) |
        del(.metadata.annotations."kubectl.kubernetes.io/last-applied-configuration") |
        del(.metadata.annotations."deployment.kubernetes.io/revision") |
        del(.status) |
        del(.spec.template.metadata.creationTimestamp)
    ' "$file" > "$output" 2>/dev/null || {
        # If yq not available, use basic filtering
        grep -v -E "(resourceVersion|uid|generation|creationTimestamp|managedFields|last-applied-configuration|deployment.kubernetes.io/revision)" "$file" > "$output"
    }
}

# Get deployment from target environment
echo "Fetching $SERVICE deployment from $NAMESPACE..."
if ! oc get deployment "$SERVICE" -n "$NAMESPACE" -o yaml > "$TEMP_DIR/${SERVICE}-${ENV}.yaml" 2>/dev/null; then
    echo -e "${RED}ERROR: Could not find deployment $SERVICE in $NAMESPACE${NC}"
    echo "Available deployments:"
    oc get deployments -n "$NAMESPACE"
    rm -rf "$TEMP_DIR"
    exit 1
fi

# Get deployment from production
echo "Fetching $SERVICE deployment from $PROD_NAMESPACE..."
if ! oc get deployment "$SERVICE" -n "$PROD_NAMESPACE" -o yaml > "$TEMP_DIR/${SERVICE}-prod.yaml" 2>/dev/null; then
    echo -e "${YELLOW}WARNING: Could not find deployment $SERVICE in production${NC}"
    echo "This might be a new service or have a different name in production."
    rm -rf "$TEMP_DIR"
    exit 0
fi

# Extract key fields
extract_key_fields "$TEMP_DIR/${SERVICE}-${ENV}.yaml" "$TEMP_DIR/${SERVICE}-${ENV}-filtered.yaml"
extract_key_fields "$TEMP_DIR/${SERVICE}-prod.yaml" "$TEMP_DIR/${SERVICE}-prod-filtered.yaml"

# Compare
echo ""
echo "Comparing configurations..."
echo "=================================================="

if diff -u "$TEMP_DIR/${SERVICE}-prod-filtered.yaml" "$TEMP_DIR/${SERVICE}-${ENV}-filtered.yaml" > "$TEMP_DIR/diff.txt"; then
    echo -e "${GREEN}✓ No significant differences found!${NC}"
else
    echo -e "${YELLOW}Found differences (- is prod, + is $ENV):${NC}"
    echo ""
    cat "$TEMP_DIR/diff.txt"
    echo ""
    echo -e "${YELLOW}Review the differences above.${NC}"
    echo "Common expected differences:"
    echo "  - namespace (9b301c-prod vs 9b301c-$ENV)"
    echo "  - replicas (prod may have more)"
    echo "  - resource limits (prod may be different)"
    echo "  - image tags (:prod vs :$ENV)"
fi

# Check specific important fields
echo ""
echo "Key Configuration Check:"
echo "=================================================="

# Check replicas
ENV_REPLICAS=$(grep -A1 "^spec:" "$TEMP_DIR/${SERVICE}-${ENV}.yaml" | grep "replicas:" | awk '{print $2}')
PROD_REPLICAS=$(grep -A1 "^spec:" "$TEMP_DIR/${SERVICE}-prod.yaml" | grep "replicas:" | awk '{print $2}')
echo "Replicas: $ENV ($ENV_REPLICAS) vs Prod ($PROD_REPLICAS)"

# Check for trigger annotation
if grep -q "image.openshift.io/triggers" "$TEMP_DIR/${SERVICE}-${ENV}.yaml"; then
    echo -e "${GREEN}✓ Trigger annotation present${NC}"
else
    echo -e "${YELLOW}⚠ No trigger annotation found${NC}"
fi

# Check image
ENV_IMAGE=$(grep "image:" "$TEMP_DIR/${SERVICE}-${ENV}.yaml" | grep -v "imagePullPolicy" | head -1 | awk '{print $2}')
PROD_IMAGE=$(grep "image:" "$TEMP_DIR/${SERVICE}-prod.yaml" | grep -v "imagePullPolicy" | head -1 | awk '{print $2}')
echo "Image: $ENV ($ENV_IMAGE)"
echo "       Prod ($PROD_IMAGE)"

# Check environment variables count
ENV_VAR_COUNT=$(grep -c "name:" "$TEMP_DIR/${SERVICE}-${ENV}.yaml" | grep -A1 "env:" || echo "0")
PROD_VAR_COUNT=$(grep -c "name:" "$TEMP_DIR/${SERVICE}-prod.yaml" | grep -A1 "env:" || echo "0")
echo "Environment variables: $ENV (~$ENV_VAR_COUNT) vs Prod (~$PROD_VAR_COUNT)"

# Save full files for manual review
echo ""
echo "Full YAML files saved to:"
echo "  $ENV: $TEMP_DIR/${SERVICE}-${ENV}.yaml"
echo "  Prod: $TEMP_DIR/${SERVICE}-prod.yaml"
echo "  Diff: $TEMP_DIR/diff.txt"
echo ""
echo "To view full files:"
echo "  less $TEMP_DIR/${SERVICE}-${ENV}.yaml"
echo "  less $TEMP_DIR/${SERVICE}-prod.yaml"
echo ""
echo "To clean up:"
echo "  rm -rf $TEMP_DIR"

# Don't auto-cleanup so user can review
# rm -rf "$TEMP_DIR"
