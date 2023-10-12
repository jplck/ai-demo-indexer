#!/bin/bash

set -e

PROJECT_NAME="$1"

if [ "$PROJECT_NAME" == "" ]; then
echo "No project name provided - aborting"
exit 0;
fi

RESOURCE_GROUP="$PROJECT_NAME-rg"
OAI_RESOURCE_NAME="openai-$PROJECT_NAME"
DI_RESOURCE_NAME="documentInt-$PROJECT_NAME"
SEARCH_RESOURCE_NAME="search-$PROJECT_NAME"
SB_RESOURCE_NAME="sb-$PROJECT_NAME"

USER_ID=$(az ad signed-in-user show --query id -o tsv)

if [ "$USER_ID" == "" ]; then
echo "No logged in user found. Use az login before running this script. - aborting"
exit 0;
fi

DI_RESOURCE_ID=$(az cognitiveservices account list --query "[?name=='$DI_RESOURCE_NAME'].id" -o tsv)
DI_USER_ROLE=${az role assignment create --assignee-object-id $USER_ID --assignee-principal-type user --role "Cognitive Services User" --scope $DI_RESOURCE_ID}

#az cli command to get primary endpoint for openai
OPENAI_ENDPOINT=$(az cognitiveservices account show --name $OAI_RESOURCE_NAME --resource-group $RESOURCE_GROUP --query "properties.endpoint" --output tsv)
DI_ENDPOINT=$(az cognitiveservices account show --name $DI_RESOURCE_NAME --resource-group $RESOURCE_GROUP --query "properties.endpoint" --output tsv)

#az cli command to get primary endpoint for azure search
SEARCH_ENDPOINT="https://$SEARCH_RESOURCE_NAME.search.windows.net"

#az cli command to get connection string for service bus
SERVICE_BUS_CONNECTION_STRING=$(az servicebus namespace authorization-rule keys list --resource-group $RESOURCE_GROUP --namespace-name $SB_RESOURCE_NAME --name RootManageSharedAccessKey --query "primaryConnectionString" --output tsv)

#create json file with all the endpoints
cat <<EOF > src/indexer/local.settings.json
{
    "IsEncrypted": false,
     "Values": {
        "AzureWebJobsStorage": "",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "OPENAI_API_ENDPOINT": "$OPENAI_ENDPOINT",
        "OPENAI_DEPLOYMENT_NAME": "model1",
        "OPENAI_EMBEDDINGS_DEPLOYMENT_NAME": "embedding",
        "DI_ENDPOINT": "$DI_ENDPOINT",
        "COGNITIVE_SEARCH_ENDPOINT": "$SEARCH_ENDPOINT",
        "COGNITIVE_SEARCH_INDEX_NAME": "cognitive-search",
        "DOCUMENT_SERVICEBUS": "$SERVICE_BUS_CONNECTION_STRING",
        "SEMANTIC_CONFIG_NAME": "semantic-config",
        "VECTOR_CONFIG_NAME": "vector-config"
    }
}