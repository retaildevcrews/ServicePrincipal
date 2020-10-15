/*
This Service Principal now is created from create-tf-vars.sh, so API permissions can be assigned

# Create an application
resource "azuread_application" "graphclient" {
    name = "${var.PROJECT_NAME}-graph-${var.ENV}"
}

# Create a service principal
resource "azuread_service_principal" "graphsp" {
  application_id = azuread_application.graphclient.application_id
  
}

resource "azuread_application_password" "graphsppwd" {
  depends_on = [azurerm_key_vault_secret.graphdappclientsecret]
  application_object_id = azuread_application.graphclient.id
  description           = "appGraphSecret"
  value                 =  azurerm_key_vault_secret.graphdappclientsecret.value
  end_date              = "2099-01-01T01:02:03Z"
}
*/
