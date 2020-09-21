
resource azurerm_container_registry svc-ppl-acr {
  name                = "${var.NAME}acr${var.ENV}"
  location            = var.LOCATION
  resource_group_name = var.ACR_RG_NAME
  admin_enabled       = false
  sku                 = "Standard"
}

resource null_resource acr-access {
  provisioner "local-exec" {
    command = "az role assignment create --scope ${azurerm_container_registry.svc-ppl-acr.id} --role acrpull --assignee ${var.ACR_SP_ID}"
  }
}

# resource null_resource acr-import {
#   provisioner "local-exec" {
#     command = "az acr import -n ${azurerm_container_registry.svc-ppl-acr.name} --source docker.io/retaildevcrew/${var.REPO}:stable --image ${var.REPO}:latest"
#   }
# }

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
