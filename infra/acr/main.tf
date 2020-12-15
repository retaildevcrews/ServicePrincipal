
resource azurerm_container_registry instance {
  name                = "${var.NAME}${var.TENANT_NAME}${var.ENV}"
  location            = var.LOCATION
  resource_group_name = var.ACR_RG_NAME
  admin_enabled       = false
  sku                 = "Standard"
}

output "acr_uri" {
  depends_on = [azurerm_container_registry.instance]
  value = azurerm_container_registry.instance.login_server
}

resource null_resource acr-access {
  provisioner "local-exec" {
    command = "az role assignment create --scope ${azurerm_container_registry.instance.id} --role acrpull --assignee ${var.ACR_SP_ID}"
  }
}

resource azurerm_container_registry_webhook instance {
  name                = "webhook"
  location            = var.LOCATION
  resource_group_name = var.ACR_RG_NAME
  registry_name       = azurerm_container_registry.instance.name
  service_uri         = "https://${var.NAME}.scm.azurewebsites.net/docker/hook"
  status              = "enabled"
  scope               = "${var.REPO}:latest"
  actions             = ["push"]
  custom_headers = {
    "Content-Type" = "application/json"
  }
}
