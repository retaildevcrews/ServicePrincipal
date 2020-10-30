# Test Matrix


| Test Case Id | SP Owners Valid | SP Owners Contents | SP Notes Valid | SP Notes Contents | Action | LKG |
|---|---|---|---|---|---|---|
| TC-1 | Y | owners | Y | valid AAD emails | overwrite LKG | N/A |
| TC-2 | N | blank | Y | valid AAD emails | Update Owners?, overwrite LKG | N/A |
| TC-3 | Y | owners | N | valid emails | Update Notes, overwrite LKG | N/A |
| TC-4 | N | blank | N | valid emails | Update Notes from LKG, Update Owners? | Exists |
| TC-5 | N | blank | N | valid emails | ? | Not Exists |
| TC-6 | Y | owners | N | blank | Update Notes from Owners, Update LKG | N/A |
| TC-7 | N | blank | N | blank | Update Notes from LKG, Update Owners from LKG? | Exists |
| TC-8 | N | blank | N | blank | ? | Not Exists |
| TC-9 | Y | owners | N | no emails | Update Notes from Owners, Update LKG | N/A |
| TC-10 | N | blank | N | no emails | Update Notes from LKG, Update Owners from LKG? | Exists |
| TC-11 | N | blank | N | no emails | ? | Not Exists | | N |