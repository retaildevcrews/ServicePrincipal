#!/bin/bash

# THIS NAMING FILE MUST BE KEPT IN SYNC WITH ANY NAMING CONVENTIONS ENCODED IN TERRAFORM CODE

TYPE=""
NAME=""
ENV=""
TENANT=""
VERBOSE=0
while getopts "hr:n:e:t:v" opt; do
    case ${opt} in
      h ) # process option h
        echo "Usage: build-resource-name [-h] "
        echo "       -h  this help message"
        echo "       -r  <resource type> (resourcegroup, storageaccount, cosmosaccount, keyvault, containerregistry)"
        echo "       -n  <resource name> "
        echo "       -e  <environment> "        
        echo "       -t  <tenant abbrev name> "
        echo "       -v - verbose"
        exit 1
        ;;
      v )
        VERBOSE=1
        ;;    
      r ) # process option t
        TYPE=${OPTARG}
        ;;
      n ) # process option n
        NAME=${OPTARG}
        ;;
      e ) # process option n
        ENV=${OPTARG}
        ;;
      t ) # process option n
        TENANT=${OPTARG}
        ;;
      ? ) 
        echo "Usage: prepare-terraform -h -t <resource type> -n <resource name>"
        exit 1
        ;;
    esac
done

if [ -z $TYPE ]
then
    echo "Missing resource type"
    exit 1
fi

if [ -z $NAME ]
then
    echo "Missing resource name"
    exit 1
fi

if [ -z $ENV ]
then
    echo "Missing environment name"
    exit 1
fi

if [ -z $TENANT ]
then
    echo "Missing tenant name"
    exit 1
fi

case $TYPE in 
    resourcegroup )
        echo "rg-${NAME,,}-${TENANT,,}-${ENV,,}"
        ;;
    storageaccount )
        echo "${NAME,,}${TENANT,,}${ENV,,}"
        ;;
    cosmosaccount )
        echo "cdb-${NAME,,}-${TENANT,,}-${ENV,,}"
        ;;
    keyvault )
        echo "kv-${NAME,,}-${TENANT,,}-${ENV,,}"
        ;;
    containerregistry )
        echo "${NAME,,}${TENANT,,}${ENV,,}"
        ;;
    functionapp )
        echo "fa-${NAME,,}-${TENANT,,}-${ENV,,}"
        ;;        
esac
exit 0
