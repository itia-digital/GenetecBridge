#!/bin/bash

RESOURCE_GROUP="up-db-servers"
LOCATION="eastus2"
FUNCTION_APP_NAME="GenetecSyncFunc"
STORAGE_ACCOUNT_NAME="updbserversperfdiag333"
APP_INSIGHTS_NAME="GenetecSyncInsights"
APP_DIAGNOSTICS_NAME="GenetecSyncDiagnostics"

# Verificar instalación de herramientas
echo "🔍 Verificando instalación de herramientas..."
if ! command -v az &> /dev/null; then
    echo "    ❌ Error: Azure CLI no está instalado."
    exit 1
fi
if ! command -v func &> /dev/null; then
    echo "    ❌ Error: Azure Functions Core Tools no está instalado. Instálalo con 'npm install -g azure-functions-core-tools@4 --unsafe-perm true'."
    exit 1
fi

echo "1️⃣ Creando grupo de recursos si no existe..."
EXISTS=$(az group exists --name $RESOURCE_GROUP)
if [ "$EXISTS" == "false" ]; then
    az group create --name $RESOURCE_GROUP --location "$LOCATION" --only-show-errors
else
    echo "    ✅ Grupo de recursos ya existe."
fi

echo "2️⃣ Creando cuenta de almacenamiento si no existe..."
EXISTS=$(az storage account show --name $STORAGE_ACCOUNT_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>/dev/null)
if [ -z "$EXISTS" ]; then
    az storage account create --name $STORAGE_ACCOUNT_NAME \
        --resource-group $RESOURCE_GROUP \
        --location "$LOCATION" \
        --sku Standard_LRS \
        --only-show-errors
else
    echo "    ✅ Cuenta de almacenamiento ya existe."
fi

echo "3️⃣ Obteniendo cadena de conexión del Storage Account..."
STORAGE_CONNECTION_STRING=$(az storage account show-connection-string --name $STORAGE_ACCOUNT_NAME --resource-group $RESOURCE_GROUP --query connectionString --output tsv)
if [ -z "$STORAGE_CONNECTION_STRING" ]; then
    echo "    ❌ Error: No se pudo obtener la cadena de conexión."
    exit 1
fi

echo "4️⃣ Creando Azure Function App si no existe..."
EXISTS=$(az functionapp list --resource-group $RESOURCE_GROUP --query "[?name=='$FUNCTION_APP_NAME'] | length(@)" --output tsv)
if [ "$EXISTS" -eq 0 ]; then
    az functionapp create --name $FUNCTION_APP_NAME \
        --resource-group $RESOURCE_GROUP \
        --storage-account $STORAGE_ACCOUNT_NAME \
        --consumption-plan-location $LOCATION \
        --runtime dotnet \
        --runtime-version 8 \
        --functions-version 4 \
        --os-type Linux \
        --only-show-errors
else
    echo "    ✅ Azure Function App ya existe."
fi

echo "5️⃣ Configurando variables de entorno en la Function App..."
az functionapp config appsettings set --name $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --settings "AzureWebJobsStorage=$STORAGE_CONNECTION_STRING" \
               "FUNCTIONS_WORKER_RUNTIME=dotnet" \
               "WEBSITE_RUN_FROM_PACKAGE=1" \
    --only-show-errors

echo "6️⃣ Configurando Application Insights si no existe..."
EXISTS=$(az monitor app-insights component show --app $APP_INSIGHTS_NAME --resource-group $RESOURCE_GROUP --query name --output tsv 2>/dev/null)
if [ -z "$EXISTS" ]; then
    az monitor app-insights component create --app $APP_INSIGHTS_NAME \
        --location "$LOCATION" \
        --resource-group $RESOURCE_GROUP \
        --kind web
else
    echo "    ✅ Application Insights ya existe."
fi

echo "7️⃣ Configurando Application Insights en la Function App..."
APP_INSIGHTS_KEY=$(az monitor app-insights component show --app $APP_INSIGHTS_NAME --resource-group $RESOURCE_GROUP --query instrumentationKey --output tsv)
if [ -z "$APP_INSIGHTS_KEY" ]; then
    echo "    ❌ Error: No se pudo obtener la clave de Application Insights."
    exit 1
fi

az functionapp config appsettings set --name $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$APP_INSIGHTS_KEY" \
               "APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=$APP_INSIGHTS_KEY" \
               "AzureWebJobsDashboard=$STORAGE_CONNECTION_STRING" \
               "AzureWebJobsStorage=$STORAGE_CONNECTION_STRING" \
               "FUNCTIONS_WORKER_RUNTIME=dotnet" \
               "WEBSITE_RUN_FROM_PACKAGE=1"

echo "8️⃣ Configurando diagnóstico de logs..."
az webapp log config --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP --web-server-logging filesystem

EXISTS=$(az monitor log-analytics workspace list --resource-group $RESOURCE_GROUP --query "[?name=='${FUNCTION_APP_NAME}-logs'] | length(@)" --output tsv)
if [ "$EXISTS" -eq 0 ]; then
    az monitor log-analytics workspace create --resource-group $RESOURCE_GROUP --location "$LOCATION" --workspace-name "${FUNCTION_APP_NAME}-logs" --only-show-errors
else
    echo "    ✅ Log Analytics Workspace ya existe."
fi

echo "9️⃣ Configurando diagnósticos en Log Analytics..."
WORKSPACE_ID=$(az monitor log-analytics workspace show --resource-group $RESOURCE_GROUP --workspace-name "${FUNCTION_APP_NAME}-logs" --query id --output tsv)
az monitor diagnostic-settings create --name $APP_DIAGNOSTICS_NAME \
    --resource $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --resource-type "Microsoft.Web/sites" \
    --logs '[{"category": "FunctionAppLogs", "enabled": true}]' \
    --workspace "$WORKSPACE_ID" \
    --only-show-errors

echo "🔟 Desplegando Azure Function..."
cd GenetecSyncFunc
func azure functionapp publish $FUNCTION_APP_NAME

echo "    ✅ ¡Azure Function desplegada correctamente!"
echo "🌍 URL de la función: https://$FUNCTION_APP_NAME.azurewebsites.net"
