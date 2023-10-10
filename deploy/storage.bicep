param location string = resourceGroup().location
param storageAccountName string
param containerNames array
param storageAccountType string = 'Standard_LRS'


resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageAccountType
  }
  kind: 'StorageV2'
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2019-06-01' = {
  name: 'default'
  parent: storageAccount
}

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01' = [for i in range(0, length(containerNames)): {
  name: containerNames[i]
  parent: blobServices
  properties: {
    publicAccess: 'None'
    metadata: {}
  }
}]

output blobStorageConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
output storageAccountName string = storageAccount.name
