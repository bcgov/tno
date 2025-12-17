# Quick Start: Migrate to Deployments

## Migration Status

- **Dev Environment**: ✅ COMPLETE - All 24 services migrated
- **Test Environment**: 🔄 READY - Preparation complete, ready to migrate
- **Prod Environment**: ⏳ PENDING - Awaiting test validation

## Recommended Approach: Use the Migration Script

The easiest way to migrate is using the automated migration script:

```bash
# 1. Login to OpenShift
oc login https://api.silver.devops.gov.bc.ca:6443

# 2. Switch to the environment you want to migrate
oc project 9b301c-dev    # or 9b301c-test, 9b301c-prod

# 3. Run the migration script
./tools/scripts/migrate-deployments.sh dev    # or test, prod
```

**The script will:**

- Migrate all services automatically
- Handle different service categories appropriately
- Validate resources before deployment
- Show logs and prompt for confirmation
- Scale down old DeploymentConfigs
- Keep old DCs for easy rollback

**Duration:** ~2 hours for all services

## Manual Single-Service Migration

If you prefer to migrate one service at a time:

```bash
# Migrate a single service
./tools/scripts/migrate-deployments.sh dev nginx

# The script handles everything:
# 1. Checks current state
# 2. Deploys new Deployment
# 3. Waits for rollout
# 4. Compares configurations
# 5. Shows logs
# 6. Prompts for confirmation
# 7. Scales down old DC
# 8. Verifies final state
```

### Verify the Deployment

```bash
# 1. Check deployment details
oc describe deployment nginx -n 9b301c-dev

# 2. Check for trigger annotation
oc get deployment nginx -n 9b301c-dev -o jsonpath='{.metadata.annotations.image\.openshift\.io/triggers}' | jq .

# 3. View logs
oc logs deployment/nginx -n 9b301c-dev --tail=50

# 4. Check events
oc get events -n 9b301c-dev --sort-by='.lastTimestamp' | grep nginx | tail -20
```

### Compare with Production

```bash
# 1. Save dev deployment
oc get deployment nginx -n 9b301c-dev -o yaml > /tmp/nginx-dev.yaml

# 2. Save production deployment
oc get deployment nginx -n 9b301c-prod -o yaml > /tmp/nginx-prod.yaml

# 3. Compare (focus on spec section)
# Ignore: metadata.generation, status, resourceVersion, uid, creationTimestamp
diff -u /tmp/nginx-prod.yaml /tmp/nginx-dev.yaml | less
```

## If Something Goes Wrong

### Check Pod Status

```bash
# See what's wrong with the pod
oc get pods -l component=nginx -n 9b301c-dev
oc describe pod <pod-name> -n 9b301c-dev
oc logs <pod-name> -n 9b301c-dev
```

### Common Issues

**1. ImagePullBackOff**

```bash
# Check if image exists
oc get imagestream nginx -n 9b301c-tools
oc describe imagestream nginx -n 9b301c-tools
```

**2. CrashLoopBackOff**

```bash
# Check logs for errors
oc logs <pod-name> -n 9b301c-dev --previous
```

**3. Pending**

```bash
# Check events
oc describe pod <pod-name> -n 9b301c-dev
# Look for resource constraints or PVC issues
```

### Rollback

```bash
# Option 1: Rollback the deployment
oc rollout undo deployment/nginx -n 9b301c-dev

# Option 2: Delete deployment and use old DeploymentConfig
oc delete deployment nginx -n 9b301c-dev
oc scale dc/nginx --replicas=1 -n 9b301c-dev
```

## Next Steps After Nginx Success

Once nginx is working, try these services in order:

### 1. Editor (Frontend App)

```bash
oc apply -k openshift/kustomize/app/editor/overlays/dev
oc rollout status deployment/editor -n 9b301c-dev
oc get route editor -n 9b301c-dev
```

### 2. Subscriber (Frontend App)

```bash
oc apply -k openshift/kustomize/app/subscriber/overlays/dev
oc rollout status deployment/subscriber -n 9b301c-dev
oc get route subscriber -n 9b301c-dev
```

### 3. Charts API

```bash
oc apply -k openshift/kustomize/charts/overlays/dev
oc rollout status deployment/charts -n 9b301c-dev
```

## Useful Commands Reference

### Viewing Resources

```bash
# List all deployments
oc get deployments -n 9b301c-dev

# List all pods
oc get pods -n 9b301c-dev

# List all routes
oc get routes -n 9b301c-dev

# List all services
oc get svc -n 9b301c-dev

# Watch pods in real-time
oc get pods -n 9b301c-dev -w
```

### Debugging

```bash
# Get pod logs
oc logs <pod-name> -n 9b301c-dev

# Follow logs in real-time
oc logs -f <pod-name> -n 9b301c-dev

# Get logs from previous container (if crashed)
oc logs <pod-name> -n 9b301c-dev --previous

# Execute command in pod
oc exec -it <pod-name> -n 9b301c-dev -- /bin/bash

# Get events
oc get events -n 9b301c-dev --sort-by='.lastTimestamp'

# Describe resource
oc describe deployment <name> -n 9b301c-dev
oc describe pod <name> -n 9b301c-dev
```

### Scaling

```bash
# Scale deployment
oc scale deployment/<name> --replicas=2 -n 9b301c-dev

# Scale to zero (stop)
oc scale deployment/<name> --replicas=0 -n 9b301c-dev
```

### Rollout Management

```bash
# Check rollout status
oc rollout status deployment/<name> -n 9b301c-dev

# View rollout history
oc rollout history deployment/<name> -n 9b301c-dev

# Rollback to previous version
oc rollout undo deployment/<name> -n 9b301c-dev

# Rollback to specific revision
oc rollout undo deployment/<name> --to-revision=2 -n 9b301c-dev

# Pause rollout
oc rollout pause deployment/<name> -n 9b301c-dev

# Resume rollout
oc rollout resume deployment/<name> -n 9b301c-dev
```

### Resource Usage

```bash
# Check CPU/Memory usage
oc adm top pods -n 9b301c-dev

# Check node resources
oc adm top nodes
```

## Testing Checklist

After deploying each service, verify:

- [ ] Pod is Running (1/1 Ready)
- [ ] No errors in logs
- [ ] Route is accessible (if applicable)
- [ ] Health endpoint responds (if applicable)
- [ ] No CrashLoopBackOff or ImagePullBackOff
- [ ] Trigger annotation is present
- [ ] Environment variables are correct
- [ ] Volumes are mounted (if applicable)

## Pro Tips

1. **Always use `--dry-run=client` first** to see what will change
2. **Watch logs during deployment** to catch errors early
3. **Keep the old DeploymentConfig** until you're confident
4. **Test routes immediately** after deployment
5. **Compare with production** to catch configuration drift
6. **Deploy one service at a time** in dev to isolate issues
7. **Take notes** on any issues for the test/prod deployments

## Getting Help

```bash
# Get help on any command
oc <command> --help

# Examples:
oc apply --help
oc rollout --help
oc logs --help
```

## Environment Variables

```bash
# Dev namespace
export DEV_NS=9b301c-dev

# Test namespace
export TEST_NS=9b301c-test

# Prod namespace
export PROD_NS=9b301c-prod

# Tools namespace (where images are stored)
export TOOLS_NS=9b301c-tools

# Then use in commands:
oc get pods -n $DEV_NS
```
