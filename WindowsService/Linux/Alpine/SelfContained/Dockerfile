FROM mcr.microsoft.com/dotnet/sdk:6.0.403-alpine3.16-amd64

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=WindowsService

ENV \
    # Unset ASPNETCORE_URLS from aspnet base image
    ASPNETCORE_URLS= \
    # Do not generate certificate
    DOTNET_GENERATE_ASPNET_CERTIFICATE=false \
    # Do not show first run text
    DOTNET_NOLOGO=true \
    # SDK version
    DOTNET_SDK_VERSION=6.0.403 \
    # Disable the invariant mode (set in base image)
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    # Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    # Skip extraction of XML docs - generally not useful within an image/container - helps performance
    NUGET_XMLDOC_MODE=skip \
    # PowerShell telemetry for docker image usage
    POWERSHELL_DISTRIBUTION_CHANNEL=PSDocker-DotnetSDK-Alpine-3.16

# Copy files
WORKDIR /app
COPY bin/release/net6.0/linux-x64/publish .

# Publish
RUN chmod +x ./WindowsService.dll && \
    # https://stackoverflow.com/questions/66963068/docker-alpine-executable-binary-not-found-even-if-in-path
    apk add gcompat
    
# Run app
ENTRYPOINT ["./WindowsService"]