FROM ubuntu:20.04

# SYSTEM TOOLS
RUN apt-get update \
    && apt-get install -y wget \
    && apt-get install -y curl \
    && apt-get install -y nuget


# Install DOTNET 3.1 (SDK+RUNTIME)
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O /var/tmp/packages-microsoft-prod.deb \
    && dpkg -i /var/tmp/packages-microsoft-prod.deb


RUN apt-get update \
  && apt-get install -y apt-transport-https \
  && apt-get update \
  && apt-get install -y dotnet-sdk-3.1


# INSTALL AZ CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# INSTALL CREDSCAN
COPY install/ /var/tmp
RUN nuget install microsoft.guardian.cli -Source /var/tmp \
    && mkdir -p /usr/bin/credscan \
    && mv /Microsoft.Guardian.Cli.0.50.0/tools/* /usr/bin/credscan \
    && chmod +x /usr/bin/credscan/guardian \
    && rm -r /Microsoft.Guardian.Cli.0.50.0 

ENV PATH="${PATH}:/usr/bin/credscan"

# CLEANUP tmp
RUN rm /var/tmp/* 


