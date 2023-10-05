@description('Location resources.')
param location string = 'westeurope'

@description('Define the project name')
param projectName string

targetScope = 'subscription'

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
      'docs'
    ]
  }
}

module indexer_storage 'storage.bicep' = {
  name: 'indexer_storage'
  scope: rg
  params: {
    location: location
    storageAccountName: 'indexerstor${projectName}'
    containerNames: []
  }
}

module indexer_func 'indexer.bicep' = {
  name: 'indexer_func'
  scope: rg
  params: {
    location: location
    storageAccountName: indexer_storage.outputs.storageAccountName
    applicationInsightsName: logging.outputs.appInsightsName
    appName: 'indexerfunc${projectName}'
    serviceBusQueueName: servicebus.outputs.serviceBusQueueName
    serviceBusNamespaceName: servicebus.outputs.serviceBusNamespaceName
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
    storagePrincipalID: indexer_storage.outputs.storagePrincipalID
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
