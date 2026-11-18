# Elasticsearch MCP Server

The Elasticsearch MCP server is a deprecated API that provides AI agents access to the MMI Elasticsearch content.
The docker image only supports a few tools, but does enable search.
The latest version of Elasticsearch if hosted in Elastic Cloud supports MCP out of the box with a subscription.

## 1. Deploy Image to Azure Container Registry

```bash
az login

# Login to ACR
az acr login --name bcgov

# Push image to ACR
docker pull docker.elastic.co/mcp/elasticsearch:0.4.6
docker tag docker.elastic.co/mcp/elasticsearch:0.4.6 bcgov-c4awhwfpcremdbga.azurecr.io/elasticsearch-mcp:0.4.6
docker push bcgov-c4awhwfpcremdbga.azurecr.io/elasticsearch-mcp:0.4.6
```

## 2. Create environment

```bash
LAW_ID=$(az monitor log-analytics workspace show -g DefaultResourceGroup-CCA \
  --workspace-name log-analytic-workspace --query customerId -o tsv)
LAW_KEY=$(az monitor log-analytics workspace get-shared-keys -g DefaultResourceGroup-CCA \
  --workspace-name log-analytic-workspace --query primarySharedKey -o tsv)

az containerapp env create --name elasticsearch-mcp-env -g Media-Monitoring-Insights \
  --location canadacentral --logs-workspace-id "$LAW_ID" --logs-workspace-key "$LAW_KEY"
```

## 3. Create token to connect to ACR

```bash
# Create a token scoped to pull on the elasticsearch-mcp repository
az acr token create \
  --name elasticsearch-mcp-pull \
  --registry bcgov \
  --repository elasticsearch-mcp content/read \
  --query "credentials.passwords[0].value" -o tsv

TOKEN_PASS=$(az acr token credential generate \
  --name elasticsearch-mcp-pull \
  --registry bcgov \
  --password1 \
  --query "passwords[0].value" -o tsv)
```

## 4. Create the app (no post-create grant needed)

```bash
ES_API_KEY={YOUR KEY}

az containerapp create \
  --name elasticsearch-mcp \
  --resource-group Media-Monitoring-Insights \
  --environment elasticsearch-mcp-env \
  --image bcgov-c4awhwfpcremdbga.azurecr.io/elasticsearch-mcp:0.4.6 \
  --registry-server bcgov-c4awhwfpcremdbga.azurecr.io \
  --registry-username elasticsearch-mcp-pull \
  --registry-password "$TOKEN_PASS" \
  --target-port 8080 \
  --ingress external \
  --command "/usr/local/bin/elasticsearch-core-mcp-server" \
  --args "http" \
  --secrets "esapikey=$ES_API_KEY" \
  --env-vars "ES_URL=https://prod-mmi.es.ca-central-1.aws.elastic-cloud.com" "ES_API_KEY=secretref:esapikey" \
  --min-replicas 1 --max-replicas 1
```

## 5. Verify

```bash
az containerapp show --name elasticsearch-mcp -g Media-Monitoring-Insights \
  --query "properties.template.containers[0].image" -o json

az containerapp revision list --name elasticsearch-mcp -g Media-Monitoring-Insights \
  --query "[].{revision:name, running:properties.runningState, replicas:properties.replicas}" -o table

FQDN=$(az containerapp show --name elasticsearch-mcp -g Media-Monitoring-Insights \
  --query properties.configuration.ingress.fqdn -o tsv)
curl "https://$FQDN/ping"
```

## Create Azure Principal with Secret Token to Connect to Azure Foundry Agent

```bash
# Log in first
az login

# If you have permission to create a service principle.
# Create the service principal + secret in one command
az ad sp create-for-rbac --name "foundry-agent-sp"

# If you do not, use an existing one.
az ad app list \
  --display-name "GCPE - Media Monitoring Insights" \
  --query "[].{name:displayName, appId:appId}" -o table

# Check if we can create a token
az ad app owner list --id "<appId>" --query "[].userPrincipalName" -o tsv

# Create a secret
az ad app credential reset --id "<appId>" --append --years 2
```

This outputs:

```json
{
  "appId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // clientId
  "displayName": "my-foundry-agent-sp",
  "password": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", // clientSecret
  "tenant": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" // tenantId
}
```

Map those to the ClientSecretCredential from before:

- appId → clientId
- password → clientSecret
- tenant → tenantId

Save the password now — it's shown only once and can't be retrieved later.
By default the secret lasts 1 year. Extend it (max 2 years) with:

```bash
az ad sp create-for-rbac --name "foundry-agent-sp" --years 2
```

### Assign the role on your Foundry project

create-for-rbac with no --scopes either assigns Contributor at the subscription or assigns nothing, depending on your tenant's defaults — neither is what you want. Scope it explicitly to the project instead:

```bash
# Assign role to service principle.
# See below to fill in the variables.
az role assignment create \
  --assignee "<appId>" \
  --role "Azure AI User" \
  --scope "/subscriptions/<sub-id>/resourceGroups/<rg>/providers/Microsoft.CognitiveServices/accounts/<resource>/projects/<project>"

# Get the full account resource ID
az cognitiveservices account show \
  --name "mmi-ai-foundry-east-us-2" \
  --resource-group "Media-Monitoring-Insights" \
  --query id -o tsv

# List the projects under that account
az cognitiveservices account project list \
  --name "mmi-ai-foundry-east-us-2" \
  --resource-group "Media-Monitoring-Insights" \
  --query "[].name" -o tsv

# Example
# Capture the values
APP_ID=$(az ad sp list --display-name "GCPE - Media Monitoring Insights" --query "[0].appId" -o tsv)
ACCOUNT_ID=$(az cognitiveservices account show \
  --name "mmi-ai-foundry-east-us-2" \
  --resource-group "Media-Monitoring-Insights" \
  --query id -o tsv)

az role assignment create \
  --assignee "$APP_ID" \
  --role "Azure AI Developer" \
  --scope "$ACCOUNT_ID/projects/mmi-ai-foundry"

# To avoid a broad default assignment, add --skip-assignment when creating the SP, then run the scoped assignment above:
az ad sp create-for-rbac --name "my-foundry-agent-sp" --skip-assignment
```

### Portal alternative

1. Microsoft Entra ID → App registrations → New registration. Name it, leave defaults, register.
2. On the app's overview page, copy the Application (client) ID and Directory (tenant) ID.
3. Certificates & secrets → New client secret. Set an expiry, then copy the Value (not the Secret ID) immediately — it's hidden after you leave the page.
4. Go to your Foundry project → Access control (IAM) → Add role assignment, pick Azure AI User, and assign it to the app you just registered.
