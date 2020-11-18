
resource azurerm_container_registry svc-ppl-acr {
  name                = "${var.NAME}acr${var.ENV}"
  location            = var.LOCATION
  resource_group_name = var.ACR_RG_NAME
  admin_enabled       = false
  sku                 = "Standard"
}

output "acr_uri" {
  depends_on = [azurerm_container_registry.svc-ppl-acr]
  value = azurerm_container_registry.svc-ppl-acr.login_server
}

resource null_resource acr-access {
  provisioner "local-exec" {
    command = "az role assignment create --scope ${azurerm_container_registry.svc-ppl-acr.id} --role acrpull --assignee ${var.ACR_SP_ID}"
  }
}

resource "azurerm_container_registry_webhook" "webhook" {
  name                = "${var.NAME}wh${var.ENV}"
  location            = var.LOCATION
  resource_group_name = var.ACR_RG_NAME
  registry_name       = azurerm_container_registry.svc-ppl-acr.name
  service_uri         = "https://${var.NAME}.scm.azurewebsites.net/docker/hook"
  status              = "enabled"
  scope               = "${var.REPO}:latest"
  actions             = ["push"]
  custom_headers = {
    "Content-Type" = "application/json"
  }
}
