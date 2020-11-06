FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["CSE.Automation/CSE.Automation.csproj", "CSE.Automation/"]
RUN dotnet restore "CSE.Automation/CSE.Automation.csproj"
COPY . .
WORKDIR /home/site/wwwroot
ARG VERSION_RUN_ID
ARG VERSION_GIT_HASH
ARG VERSION_QUALITY_TAG
RUN dotnet publish -p:VersionRunId=${VERSION_RUN_ID} -p:VersionGitHash=${VERSION_GIT_HASH} -p:VersionQualityTag=${VERSION_QUALITY_TAG} /src/CSE.Automation/*.csproj --output .

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 as final
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]
EXPOSE 80
