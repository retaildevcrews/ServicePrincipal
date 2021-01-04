#!/bin/bash

# curl -fsSL https://apt.releases.hashicorp.com/gpg | sudo apt-key add -
# sudo apt-add-repository "deb [arch=amd64] https://apt.releases.hashicorp.com $(lsb_release -cs) main"
# sudo apt-get update && sudo apt-get install terraform

curl -O https://releases.hashicorp.com/terraform/0.13.5/terraform_0.13.5_linux_386.zip && unzip terraform_0.13.5_linux_386.zip && mkdir bin && mv terraform bin/
