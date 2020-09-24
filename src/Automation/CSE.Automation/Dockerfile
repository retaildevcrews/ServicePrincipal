#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
ARG SP_AUTOMATION_VERSION
# Embed them in base
ENV SP_AUTOMATION_VERSION=${SP_AUTOMATION_VERSION}
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
ARG SP_AUTOMATION_VERSION
ENV SP_AUTOMATION_VERSION=${SP_AUTOMATION_VERSION}
WORKDIR /src
COPY ["CSE.Automation/CSE.Automation.csproj", "CSE.Automation/"]
RUN dotnet restore "CSE.Automation/CSE.Automation.csproj"
COPY . .
WORKDIR "/src/CSE.Automation"
# If SP_AUTOMATION_VERSION is not specified, then don't use /p:Version argument
RUN [ ! -z ${SP_AUTOMATION_VERSION} ] && echo Specified Version: ${SP_AUTOMATION_VERSION} \
    && VER_ARG="/p:Version=${SP_AUTOMATION_VERSION}" \
    || echo "Version not specified in docker arg." \
    && dotnet build   "CSE.Automation.csproj" -c Release -o /app/build   ${VER_ARG} \
    && dotnet publish "CSE.Automation.csproj" -c Release -o /app/publish ${VER_ARG}

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=build /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true