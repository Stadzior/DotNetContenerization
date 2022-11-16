FROM mcr.microsoft.com/windows/servercore:20H2

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=ConsoleApp

ENV \
    # Unset ASPNETCORE_URLS from aspnet base image
    ASPNETCORE_URLS= \
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

RUN powershell -Command " \
        $ErrorActionPreference = 'Stop'; \
        $ProgressPreference = 'SilentlyContinue'; \
        # Retrieve .NET SDK
        $sdk_version = '6.0.402'; \
        Invoke-WebRequest -OutFile dotnet.zip https://dotnetcli.azureedge.net/dotnet/Sdk/$sdk_version/dotnet-sdk-$sdk_version-win-x64.zip; \
        mkdir dotnet; \
        tar -oxzf dotnet.zip -C dotnet; \
        Remove-Item -Force dotnet.zip; \
        # Install PowerShell global tool
        $powershell_version = '7.2.7'; \
        Invoke-WebRequest -OutFile PowerShell.Windows.x64.$powershell_version.nupkg https://pwshtool.blob.core.windows.net/tool/$powershell_version/PowerShell.Windows.x64.$powershell_version.nupkg; \
        & \dotnet\dotnet tool install --add-source . --tool-path \powershell --version $powershell_version PowerShell.Windows.x64; \
        & \dotnet\dotnet nuget locals all --clear; \
        Remove-Item -Force PowerShell.Windows.x64.$powershell_version.nupkg; \
        Remove-Item -Path \powershell\.store\powershell.windows.x64\$powershell_version\powershell.windows.x64\$powershell_version\powershell.windows.x64.$powershell_version.nupkg -Force; \
        # Workaround for https://github.com/dotnet/sdk/issues/18410
        \dotnet\dotnet build-server shutdown; \
        $Path = [Environment]::GetEnvironmentVariable('PATH', 'Machine') + ';C:\dotnet;C:\dotnet-runtime'; \
        [Environment]::SetEnvironmentVariable('Path', $Path, 'Machine')"

# Copy files
COPY Program.cs C:/app/Program.cs
COPY ConsoleApp.csproj C:/app/ConsoleApp.csproj

# Publish
RUN powershell -Command "dotnet publish C:\app\ConsoleApp.csproj -c release -r win-x64 --no-self-contained"

# Run app
ENTRYPOINT ["dotnet", "C:/app/bin/release/net6.0/win-x64/publish/ConsoleApp.dll"]