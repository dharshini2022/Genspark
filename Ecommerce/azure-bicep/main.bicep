param location string = resourceGroup().location
param keyVaultName string = 'shophive-kv'
param storageAccountName string = 'shophive'
param postgresServerName string = 'shophive-db'
param postgresAdminUser string = 'shophiveadmin'
@secure()
param postgresAdminPassword string
param containerRegistryName string = 'acrshophive'
param containerAppName string = 'shophive-api'
param staticWebAppName string = 'shophive-swa'
param logAnalyticsWorkspaceName string = 'shophive-log'
param containerAppEnvName string = 'shophive-env'
param staticWebAppLocation string = 'eastasia'

// 1. Storage Account & Blob Containers
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: { allowBlobPublicAccess: true }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource productsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'products'
  properties: { publicAccess: 'Blob' }
}

resource reviewsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'reviews'
  properties: { publicAccess: 'Blob' }
}

resource logsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'logs'
  properties: { publicAccess: 'None' } // Logs should be private
}

// 2. PostgreSQL Server & Configuration
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: postgresServerName
  location: location
  sku: { name: 'Standard_B1ms', tier: 'Burstable' }
  properties: {
    administratorLogin: postgresAdminUser
    administratorLoginPassword: postgresAdminPassword
    version: '15'
    storage: { storageSizeGB: 32 }
    backup: { backupRetentionDays: 7, geoRedundantBackup: 'Disabled' }
  }
}

resource postgresDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: 'ecommercedb'
}

resource firewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = {
  parent: postgresServer
  name: 'AllowAllAzureIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// 3. Azure Key Vault & Secret Store
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: { family: 'A', name: 'standard' }
    tenantId: subscription().tenantId
    enableSoftDelete: true
    accessPolicies: [] // Dynamically configured below
  }
}

resource secretDbConn 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--Default'
  properties: {
    value: 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=ecommercedb;Username=${postgresAdminUser};Password=${postgresAdminPassword};SSL Mode=Require;'
  }
}

resource secretStorageUri 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'AzureStorage--BlobServiceUri'
  properties: {
    value: storageAccount.properties.primaryEndpoints.blob
  }
}

resource secretStorageConn 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'AzureStorage--ConnectionString'
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
  }
}

// 4. Container App Workspace & Environments
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: { sku: { name: 'PerGB2018' } }
}

resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

// Reference the pre-existing Container Registry for Pull authentication
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}

// 5. Container App Deployment
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: '${containerRegistryName}.azurecr.io'
          username: containerRegistry.name
          passwordSecretRef: 'acr-password'
        }
      ]
      secrets: [
        {
          name: 'acr-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'shophive-api'
          image: '${containerRegistryName}.azurecr.io/shophive-api:v1'
          env: [
            {
              name: 'KeyVault__VaultUri'
              value: keyVault.properties.vaultUri
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 2
      }
    }
  }
}

// 6. IAM Identity / Access Policy Declarations

// Role assignments are omitted as Contributor license does not permit role assignments.
// Managed access is handled via ACR Credentials and Key Vault connection strings.

// Key Vault Policy Bindings
resource kvAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: containerApp.identity.principalId
        permissions: { secrets: [ 'get', 'list' ] }
      }
    ]
  }
}

// 7. Static Web App (Frontend)
resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: staticWebAppName
  location: location
  sku: { name: 'Free', tier: 'Free' }
  properties: {}
}