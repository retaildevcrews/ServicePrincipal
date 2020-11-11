FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["CSE.Automation/CSE.Automation.csproj", "CSE.Automation/"]
RUN dotnet restore "CSE.Automation/CSE.Automation.csproj"
COPY . .
WORKDIR /home/site/wwwroot
ARG VERSION_PREFIX
ARG VERSION_SUFFIX
RUN dotnet publish -p:VersionPrefix=${VERSION_PREFIX} -p:VersionSuffix=${VERSION_SUFFIX} /src/CSE.Automation/*.csproj --output .

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 as final
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]
EXPOSE 80
