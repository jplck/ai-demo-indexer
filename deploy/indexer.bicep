@description('The name of the function app that you wish to create.')
param appName string

@description('Location for all resources.')
param location string = resourceGroup().location

param runtime string = 'dotnet'

param webJobStorageAccountName string

param documentsToIndexStorageAccountName string

param applicationInsightsName string

param serviceBusQueueName string

param serviceBusNamespaceName string

var hostingPlanName = '${appName}-plan'

var functionWorkerRuntime = runtime

var systemTopicName = 'storageeventtopic'
var blobToFuncEventSubscriptionName = 'blobToFuncEventSubscription'

resource webJobStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' existing = {
  name: webJobStorageAccountName
}

resource documentsToIndexStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' existing = {
  name: documentsToIndexStorageAccountName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

resource systemTopic 'Microsoft.EventGrid/systemTopics@2021-12-01' = {
  dependsOn: [
    functionApp
  ]
  name: systemTopicName
  location: location
  properties: {
    source: documentsToIndexStorageAccount.id
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

resource eventSubscription 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2021-12-01' = {
  parent: systemTopic
  name: blobToFuncEventSubscriptionName
  properties: {
    eventDeliverySchema: 'CloudEventSchemaV1_0'
    destination: {
      properties: {
        resourceId: resourceId('Microsoft.ServiceBus/namespaces/queues', serviceBusNamespaceName, serviceBusQueueName)
      }
      endpointType: 'ServiceBusQueue'
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Storage.BlobCreated'
      ]
    }
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: appName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${webJobStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${webJobStorageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${webJobStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${webJobStorageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(appName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionWorkerRuntime
        }
      ]
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
    httpsOnly: false
  }
}
