
resource azurerm_storage_queue evaluate-queue {
  depends_on = [
    var.STORAGE_ACCOUNT_DONE
  ]
  name                 = "evaluate"
  storage_account_name = var.STORAGE_ACCOUNT_NAME
}

resource azurerm_storage_queue update-queue {
  depends_on = [
    var.STORAGE_ACCOUNT_DONE
  ]
  name                 = "update"
  storage_account_name = var.STORAGE_ACCOUNT_NAME
}

output "EVALUATE_QUEUE_NAME" {
  depends_on  = [azurerm_storage_queue.evaluate-queue]
  value       = azurerm_storage_queue.evaluate-queue.name
  description = "Evaluate queue name"
}

output "UPDATE_QUEUE_NAME" {
  depends_on  = [azurerm_storage_queue.update-queue]
  value       = azurerm_storage_queue.update-queue.name
  description = "Update queue name"
}
