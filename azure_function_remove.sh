#!/bin/bash

# Configuration
RESOURCE_GROUP="up-db-servers"
FUNCTION_APP_NAME="GenetecSyncFunc"
APP_INSIGHTS_NAME="GenetecSyncInsights"
APP_DIAGNOSTICS_NAME="GenetecSyncDiagnostics"
APP_SERVICE_PLAN_NAME="GenetecSyncPlan"
LOG_ANALYTICS_WORKSPACE_NAME="${FUNCTION_APP_NAME}-logs"

# Verify Azure CLI installation
echo "🔍 Verifying Azure CLI installation..."
if ! command -v az &> /dev/null; then
    echo "    ❌ Error: Azure CLI is not installed."
    exit 1
fi

# Delete Azure Function App
echo "🗑 Removing Azure Function App: $FUNCTION_APP_NAME..."
EXISTS=$(az functionapp show --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>/dev/null)
if [ -n "$EXISTS" ]; then
    az functionapp delete --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP --only-show-errors
    echo "    ✅ Function App '$FUNCTION_APP_NAME' removed."
else
    echo "    ⚠ Function App '$FUNCTION_APP_NAME' does not exist."
fi

# Delete App Service Plan
echo "🗑 Removing App Service Plan: $APP_SERVICE_PLAN_NAME..."
EXISTS=$(az appservice plan show --name $APP_SERVICE_PLAN_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>/dev/null)
if [ -n "$EXISTS" ]; then
    az appservice plan delete --name $APP_SERVICE_PLAN_NAME --resource-group $RESOURCE_GROUP --yes --only-show-errors
    echo "    ✅ App Service Plan '$APP_SERVICE_PLAN_NAME' removed."
else
    echo "    ⚠ App Service Plan '$APP_SERVICE_PLAN_NAME' does not exist."
fi

# Delete Application Insights
echo "🗑 Removing Application Insights: $APP_INSIGHTS_NAME..."
EXISTS=$(az monitor app-insights component show --app $APP_INSIGHTS_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>/dev/null)
if [ -n "$EXISTS" ]; then
    az monitor app-insights component delete --app $APP_INSIGHTS_NAME --resource-group $RESOURCE_GROUP
    echo "    ✅ Application Insights '$APP_INSIGHTS_NAME' removed."
else
    echo "    ⚠ Application Insights '$APP_INSIGHTS_NAME' does not exist."
fi

# Delete Log Analytics Workspace
echo "🗑 Removing Log Analytics Workspace: $LOG_ANALYTICS_WORKSPACE_NAME..."
EXISTS=$(az monitor log-analytics workspace show --resource-group $RESOURCE_GROUP --workspace-name $LOG_ANALYTICS_WORKSPACE_NAME --query name --output tsv 2>/dev/null)
if [ -n "$EXISTS" ]; then
    az monitor log-analytics workspace delete --resource-group $RESOURCE_GROUP --workspace-name $LOG_ANALYTICS_WORKSPACE_NAME --yes --only-show-errors
    echo "    ✅ Log Analytics Workspace '$LOG_ANALYTICS_WORKSPACE_NAME' removed."
else
    echo "    ⚠ Log Analytics Workspace '$LOG_ANALYTICS_WORKSPACE_NAME' does not exist."
fi

# Delete Application Diagnostics (APP_DIAGNOSTICS_NAME)
echo "🗑 Removing Application Diagnostics: $APP_DIAGNOSTICS_NAME..."
EXISTS=$(az monitor diagnostic-settings list --resource $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP --resource-type "Microsoft.Web/sites" --query "value[?name=='$APP_DIAGNOSTICS_NAME'].name" --output tsv 2>/dev/null)
if [ -n "$EXISTS" ]; then
    az monitor diagnostic-settings delete --name $APP_DIAGNOSTICS_NAME --resource $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP --resource-type "Microsoft.Web/sites" --only-show-errors
    echo "    ✅ Application Diagnostics '$APP_DIAGNOSTICS_NAME' removed."
else
    echo "    ⚠ Application Diagnostics '$APP_DIAGNOSTICS_NAME' does not exist."
fi

echo "🎉 All specified resources have been deleted successfully!"