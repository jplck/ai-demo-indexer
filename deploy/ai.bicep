param location string = 'westeurope'
param openaiDeploymentName string
param documentIntDeploymentName string
param openAISku string = 'S0'
param searchSku string = 'standard'
param docIntSku string = 'S0'
param projectName string
param indexerStorageAccountName string

resource indexerStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' existing = {
  name: indexerStorageAccountName
}

resource openAIAccount 'Microsoft.CognitiveServices/accounts@2022-03-01' = {
  name: openaiDeploymentName
  location: location
  kind: 'OpenAI'
  sku: {
    name: openAISku
  }
  properties: {
    customSubDomainName: ''
    publicNetworkAccess: 'Enabled'
  }
}

resource documentIntAccount 'Microsoft.CognitiveServices/accounts@2022-03-01' = {
  name: documentIntDeploymentName
  location: location
  kind: 'FormRecognizer'
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: docIntSku
  }
  properties: {
    customSubDomainName: 'di-${projectName}'
    publicNetworkAccess: 'Enabled'
  }
}

resource deployment 'Microsoft.CognitiveServices/accounts/deployments@2022-10-01' = {
  parent: openAIAccount
  name: 'model1'
  properties: {
    model: {
      name: 'gpt-35-turbo'
      version: '0301'
      format: 'OpenAI'
    }
    scaleSettings: {
      scaleType: 'Standard'
    }
  }
}

resource search 'Microsoft.Search/searchServices@2021-04-01-preview' = {
  name: 'search-${projectName}'
  location: location
  sku: {
    name: searchSku
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    semanticSearch: 'free'
  }
}

var roleDefinitionIDs = [
  '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1' //Storage Blob Data Reader
]

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for roleDefinitionID in roleDefinitionIDs: {
  name: guid(documentIntAccount.name, roleDefinitionID, resourceGroup().id)
  scope: indexerStorageAccount
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionID)
    principalId: documentIntAccount.identity.principalId
  }
}]

output openaiDeploymentEndpoint string = openAIAccount.properties.endpoint
