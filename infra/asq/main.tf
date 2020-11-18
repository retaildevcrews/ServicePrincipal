
resource azurerm_storage_queue instance {
  for_each = toset(var.QUEUE_NAMES)

  depends_on = [ var.STORAGE_ACCOUNT_DONE ]
  name                 = each.key
  storage_account_name = var.STORAGE_ACCOUNT_NAME
}

# output "DISCOVER_QUEUE_NAME" {
#   depends_on  = [azurerm_storage_queue.evaluate-queue]
#   value       = azurerm_storage_queue.discover-queue.name
#   description = "Discover queue name"
# }

# output "EVALUATE_QUEUE_NAME" {
#   depends_on  = [azurerm_storage_queue.evaluate-queue]
#   value       = azurerm_storage_queue.evaluate-queue.name
#   description = "Evaluate queue name"
# }

# output "UPDATE_QUEUE_NAME" {
#   depends_on  = [azurerm_storage_queue.update-queue]
#   value       = azurerm_storage_queue.update-queue.name
#   description = "Update queue name"
# }
