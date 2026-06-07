@description('Azure region for resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('App Service Plan name.')
param appServicePlanName string = 'asp-within-np-001'

@description('App Service (Web API) name. Must be globally unique.')
param webAppName string = 'webapp-within-np-001'

@description('App Service Plan SKU. B1 = Basic single-instance.')
@allowed([
  'F1'
  'B1'
  'B2'
  'S1'
  'P1v3'
])
param skuName string = 'F1'

@description('JWT issuer baked into the API config.')
param jwtIssuer string = 'WithinAPI'

@description('JWT audience baked into the API config.')
param jwtAudience string = 'WithinApp'

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: skuName
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      alwaysOn: skuName != 'F1'
      http20Enabled: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'Jwt__Issuer'
          value: jwtIssuer
        }
        {
          name: 'Jwt__Audience'
          value: jwtAudience
        }
      ]
    }
  }
}

output webAppName string = webApp.name
output defaultHostName string = webApp.properties.defaultHostName
output appServicePlanId string = plan.id
output webAppResourceId string = webApp.id
