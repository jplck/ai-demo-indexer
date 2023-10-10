param location string = resourceGroup().location
param documentsToIndexStorageAccountName string
param serviceBusQueueName string
param serviceBusNamespaceName string
param documentToIndexStorageContainerName string

var systemTopicName = 'storageeventtopic'
var blobToFuncEventSubscriptionName = 'blobToFuncEventSubscription'

resource documentsToIndexStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' existing = {
  name: documentsToIndexStorageAccountName
}

resource systemTopic 'Microsoft.EventGrid/systemTopics@2021-12-01' = {
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
      subjectBeginsWith: '/blobServices/default/containers/${documentToIndexStorageContainerName}/'
    }
  }
}
