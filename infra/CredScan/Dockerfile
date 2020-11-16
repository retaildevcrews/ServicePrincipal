FROM mcr.microsoft.com/dotnet/sdk:3.1-focal 

# SYSTEM TOOLS
RUN apt-get update \
    && apt-get install -y nuget


# Install DOTNET 3.1 (SDK+RUNTIME)
# RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O /var/tmp/packages-microsoft-prod.deb \
#     && dpkg -i /var/tmp/packages-microsoft-prod.deb


# RUN apt-get update \
#   && apt-get install -y apt-transport-https \
#   && apt-get update \
#   && apt-get install -y dotnet-sdk-3.1


# INSTALL AZ CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# INSTALL CREDSCAN
# NOTE: CredScan is packaged in the Guardian nuget available from the internal SecurityTools nuget feed
# Log into Azure (MSFT creds) and download using the commands below.  This must be done outside of this dockerfile
#   az login
#   az artifacts universal download --organization https://securitytools.visualstudio.com/ --feed Guardian --name Microsoft.Guardian.Cli --version 0.50.0 --path ./install

COPY install/ /var/tmp
RUN nuget install microsoft.guardian.cli -Source /var/tmp \
    && mkdir -p /usr/bin/credscan \
    && mv /Microsoft.Guardian.Cli.0.50.0/tools/* /usr/bin/credscan \
    && chmod +x /usr/bin/credscan/guardian \
    && rm -r /Microsoft.Guardian.Cli.0.50.0 

ENV PATH="${PATH}:/usr/bin/credscan"

# CLEANUP tmp
RUN rm /var/tmp/* 


