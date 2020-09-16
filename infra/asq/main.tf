
resource azurerm_storage_queue tracking-queue {
   depends_on = [
    var.STORAGE_ACCOUNT_DONE
  ]
  name                 = "${var.NAME}-sq-trackingupdate-${var.ENV}"
  storage_account_name = var.STORAGE_ACCOUNT 
}

resource azurerm_storage_queue aad-queue {
  depends_on = [
    var.STORAGE_ACCOUNT_DONE
  ]
  name                 = "${var.NAME}-sq-aadupdate-${var.ENV}"
  storage_account_name = var.STORAGE_ACCOUNT 
}