# Installer image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.10-nanoserver-1809

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=MinimalApi

ENV \
    # Unset ASPNETCORE_URLS from aspnet base image
    ASPNETCORE_URLS= \
    # Ensure we listen on any IP Address 
    DOTNET_URLS=http://+:5000 \
    # Do not generate certificate
    DOTNET_GENERATE_ASPNET_CERTIFICATE=false \
    # Do not show first run text
    DOTNET_NOLOGO=true \
    # SDK version
    DOTNET_SDK_VERSION=6.0.403 \
    # Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    # Skip extraction of XML docs - generally not useful within an image/container - helps performance
    NUGET_XMLDOC_MODE=skip

# Copy files
WORKDIR /app
COPY bin/release/net6.0/win-x64/publish .

# Run app
ENTRYPOINT ["MinimalApi.exe"]