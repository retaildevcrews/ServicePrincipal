
resource azurerm_storage_queue instance {
  for_each = toset(var.QUEUE_NAMES)

  name                 = each.key
  storage_account_name = var.STORAGE_ACCOUNT_NAME
}
