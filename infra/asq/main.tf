
resource azurerm_storage_queue tracking-queue {
   depends_on = [
    var.STORAGE_ACCOUNT_DONE
  ]
  name                 = "${var.NAME}-sq-trackingupdate-${var.ENV}"
  storage_account_name = var.STORAGE_ACCOUNT_NAME
}

resource azurerm_storage_queue aad-queue {
  depends_on = [
    var.STORAGE_ACCOUNT_DONE
  ]
  name                 = "${var.NAME}-sq-aadupdate-${var.ENV}"
  storage_account_name = var.STORAGE_ACCOUNT_NAME 
}

output "AAD_QUEUE_NAME" {
  depends_on  = [azurerm_storage_queue.aad-queue]
  value       = azurerm_storage_queue.aad-queue.name
  description = "AAD queue name"
}

output "TRACKING_QUEUE_NAME" {
  depends_on  = [azurerm_storage_queue.tracking-queue]
  value       = azurerm_storage_queue.tracking-queue.name
  description = "Tracking queue name"
}