FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["CSE.Automation/CSE.Automation.csproj", "CSE.Automation/"]
RUN dotnet restore "CSE.Automation/CSE.Automation.csproj"
COPY . .
WORKDIR /home/site/wwwroot
ARG SP_AUTOMATION_VERSION
ARG ACR_REG_REPO_URL
ARG SERVICE_PRINCIPAL_ID
RUN echo ${ACR_REG_REPO_URL}
RUN echo ${SERVICE_PRINCIPAL_ID}
RUN [ ! -z ${SP_AUTOMATION_VERSION} ] && echo Specified Version: ${SP_AUTOMATION_VERSION} \
    && VER_ARG="/p:Version=${SP_AUTOMATION_VERSION}" \
    || echo "Version not specified in docker arg." \
    && dotnet publish /src/CSE.Automation/*.csproj --output .

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 as final
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]
# Enable Viewing Versions From Container Env Vars
ARG SP_AUTOMATION_VERSION
ARG SP_AUTOMATION_GITHASH
ENV SP_AUTOMATION_VERSION=${SP_AUTOMATION_VERSION}
ENV SP_AUTOMATION_GITHASH=${SP_AUTOMATION_GITHASH}
EXPOSE 80
