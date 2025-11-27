# TLS Certificate Protection During Migration

## Overview

Routes with custom TLS certificates need special handling during migration. The migration script automatically backs up and restores certificates to prevent loss.

## Migration Status

### Dev Environment - COMPLETE ✅

- ✅ All routes migrated successfully
- ✅ TLS certificates preserved
- ✅ Routes verified accessible

**Routes with Custom TLS Certificates:**

- `editor-tls` → `dev.editor.mmi.gov.bc.ca`
- `subscriber-tls` → `dev.mmi.gov.bc.ca`

### Test Environment - READY 🔄

Check routes with certificates:

```bash
oc get routes -n 9b301c-test -o json | jq '.items[] | select(.spec.tls.certificate != null) | .metadata.name'
```

Expected routes:

- `editor-tls` → `test.editor.mmi.gov.bc.ca`
- `subscriber-tls` → `test.mmi.gov.bc.ca`

### Prod Environment - PENDING ⏳

Check routes with certificates:

```bash
oc get routes -n 9b301c-prod -o json | jq '.items[] | select(.spec.tls.certificate != null) | .metadata.name'
```

## Protection Mechanisms Implemented

### 1. Migration Script Auto-Backup/Restore

The `migrate-deployments.sh` script now:

1. **Backs up** all TLS certificates from routes before applying kustomize
2. **Checks** if certificates were removed after applying
3. **Restores** certificates automatically if they're missing
4. **Cleans up** temporary backup files

Location: Lines 313-357 in `tools/scripts/migrate-deployments.sh`

### 2. .gitignore Protection

Added entries to prevent accidental commits of certificate files:

```
openshift/kustomize/.tls-certs/*.crt
openshift/kustomize/.tls-certs/*.key
openshift/kustomize/.tls-certs/*.pem
```

### 3. Documentation

Created `openshift/kustomize/.tls-certs/README.md` with:

- List of routes with certificates
- Backup/restore procedures
- Alternative solutions

## Manual Verification

After any migration, verify certificates are intact:

```bash
# Check if certificate exists
oc get route editor-tls -n 9b301c-dev -o jsonpath='{.spec.tls.certificate}' | wc -c
# Should return > 100 (certificate exists)

# View certificate details
oc get route editor-tls -n 9b301c-dev -o jsonpath='{.spec.tls.certificate}' | openssl x509 -text -noout
```

## Manual Restore (if needed)

If certificates are lost despite protections:

```bash
# 1. Extract from a working route (e.g., subscriber-tls)
oc get route subscriber-tls -n 9b301c-dev -o jsonpath='{.spec.tls.certificate}' > /tmp/cert.pem
oc get route subscriber-tls -n 9b301c-dev -o jsonpath='{.spec.tls.key}' > /tmp/key.pem

# 2. Restore to affected route
oc patch route editor-tls -n 9b301c-dev --type=json -p='[
  {"op": "add", "path": "/spec/tls/certificate", "value": "'"$(cat /tmp/cert.pem)"'"},
  {"op": "add", "path": "/spec/tls/key", "value": "'"$(cat /tmp/key.pem)"'"}
]'

# 3. Verify
curl -I https://dev.editor.mmi.gov.bc.ca
```
