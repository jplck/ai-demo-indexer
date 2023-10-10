@description('Location resources.')
param location string = 'westeurope'

@description('Define the project name')
param projectName string

targetScope = 'subscription'

var docsToIndexContainerName = 'docstoindex'

resource rg 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: '${projectName}-rg'
  location: location
}

module logging 'logging.bicep' = {
  name: 'logging'
  scope: rg
  params: {
    location: location
    logAnalyticsWorkspaceName: 'log-${projectName}'
    applicationInsightsName: 'appi-${projectName}'
  }
}

module docs_storage 'storage.bicep' = {
  name: 'docs_storage'
  scope: rg
  params: {
    location: location
    storageAccountName: 'docs${projectName}'
    containerNames: [
      docsToIndexContainerName
    ]
  }
}

module indexer_func_storage 'storage.bicep' = {
  name: 'indexer_func_storage'
  scope: rg
  params: {
    location: location
    storageAccountName: 'indexerfuncstor${projectName}'
    containerNames: []
  }
}

module indexer_func 'indexer.bicep' = {
  name: 'indexer_func'
  scope: rg
  params: {
    location: location
    webJobStorageAccountName: indexer_func_storage.outputs.storageAccountName
    applicationInsightsName: logging.outputs.appInsightsName
    appName: 'indexerfunc${projectName}'
  }
}

module ai 'ai.bicep' = {
  name: 'ai'
  scope: rg
  params: {
    location: location
    openaiDeploymentName: 'openai-${projectName}'
    documentIntDeploymentName: 'documentInt-${projectName}'
    projectName: projectName
    indexerStorageAccountName: indexer_func_storage.outputs.storageAccountName
  }
}

module servicebus 'servicebus.bicep' = {
  name: 'servicebus'
  scope: rg
  params: {
    location: location
    serviceBusNamespaceName: 'sb-${projectName}'
    serviceBusQueueName: 'docsevents'
  }
}

module event_subscriptions 'event_subscriptions.bicep' = {
  name: 'event_subscriptions'
  scope: rg
  params: {
    location: location
    serviceBusNamespaceName: servicebus.outputs.serviceBusNamespaceName
    serviceBusQueueName: servicebus.outputs.serviceBusQueueName
    documentsToIndexStorageAccountName: docs_storage.outputs.storageAccountName
    documentToIndexStorageContainerName: docsToIndexContainerName
  }
}
