# escape=`

# Setup shared variables
ARG ERLANG_VERSION=25.0
ARG RABBITMQ_VERSION=3.11.4

# Use server core to support erlang install
FROM mcr.microsoft.com/windows/servercore:20H2 as source

# Setup PowerShell as default Run Shell
SHELL ["PowerShell.exe", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue'; "]

# Environment Variables (ARGs needed to see outer scope ARGs)
ARG ERLANG_VERSION
ARG RABBITMQ_VERSION
ENV ERLANG_VERSION=$ERLANG_VERSION `
    RABBITMQ_VERSION=$RABBITMQ_VERSION

# Install Erlang OTP & RabbitMQ
RUN Write-Host -Object 'Downloading Erlang OTP' ; `
    $erlangInstaller = Join-Path -Path $env:Temp -ChildPath 'otp_win64.exe' ; `
    Invoke-WebRequest -UseBasicParsing -Uri $('http://erlang.org/download/otp_win64_{0}.exe' -f $env:ERLANG_VERSION) -OutFile $erlangInstaller ; `
    Unblock-File -Path $erlangInstaller ; `
    Write-Host -Object 'Installing Erlang OTP' ; `
    Start-Process -NoNewWindow -Wait -FilePath $erlangInstaller -ArgumentList /S, /D=c:\erlang ; `
    Write-Host -Object 'Removing Erlang OTP Installer' ; `
    Remove-Item -Path $erlangInstaller ; `
    Write-Host -Object 'Done Installing Erlang OTP' ; `
    Write-Host -Object 'Downloading RabbitMQ Zip' ; `
    $rabbitZip = Join-Path -Path $env:Temp -ChildPath 'rabbitmq.zip' ; `
    Invoke-WebRequest -UseBasicParsing -Uri $('https://github.com/rabbitmq/rabbitmq-server/releases/download/v{0}/rabbitmq-server-windows-{0}.zip' -f $env:RABBITMQ_VERSION) -OutFile $rabbitZip ; `
    Unblock-File -Path $rabbitZip ; `
    Write-Host -Object 'Extracting RabbitMQ' ; `
    Expand-Archive -Path $rabbitZip -DestinationPath c:\ ; `
    Rename-Item c:\rabbitmq_server-$env:RABBITMQ_VERSION c:\rabbitmq ; `
    Write-Host -Object 'Removing RabbitMQ Zip' ; `
    Remove-Item -Path $rabbitZip ; `
    Write-Host -Object 'Done Installing RabbitMQ'

# Start from nano server
FROM mcr.microsoft.com/dotnet/aspnet:6.0.10-nanoserver-1809

# # Copy erlang and rabbit
COPY --from=source C:\erlang\ c:\erlang\
COPY --from=source C:\rabbitmq\ c:\rabbitmq\

# # Environment Variables (ARGs needed to see outer scope ARGs)
ENV ERLANG_HOME=c:\erlang `
    RABBITMQ_HOME=c:\rabbitmq `
    RABBITMQ_BASE=c:\data `
    ERLANG_VERSION=$ERLANG_VERSION

# # setup working directory
WORKDIR $RABBITMQ_HOME\sbin

USER ContainerAdministrator

# run external command when container starts to allow for additional setup
# CMD rabbitmq-server.bat
