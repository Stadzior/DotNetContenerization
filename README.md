# DotNetContenerization
Repo for traning purposes illustrating containerization of various types of dotnet apps.

# Console App
Simple console app which prints to console "Running!" every 100ms.
Created images:
- Alpine linux with dotnet & powershell **1.05GB**:
```
cd [PATH_TO_REPO]/ConsoleApp/Linux/Alpine/DotNetAndPowerShell
docker image build -t alpine-dotnet-consoleapp -f Dockerfile .
docker container run -dit --name alpine-dotnet-consoleapp-1 alpine-dotnet-consoleapp
```
- Alpine linux with self-contained dotnetapp (dotnet & powershell must be installed in host) **679MB**:
```
cd [PATH_TO_REPO]/ConsoleApp/Linux/Alpine/SelfContained
dotnet publish ConsoleApp.csproj -c release -r linux-x64 --self-contained
docker image build -t alpine-dotnet-consoleapp:2 -f Dockerfile .
docker container run -dit --name alpine-dotnet-consoleapp-2 alpine-dotnet-consoleapp:2
```
- Windows Nano with dotnet & powershell **0B(!?)**:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/Nano/DotNetAndPowerShell
docker image build -t nano-dotnet-consoleapp -f Dockerfile .
docker container run -dit --name nano-dotnet-consoleapp-1 nano-dotnet-consoleapp
```
- Windows Nano with self-contained dotnetapp **0B(!?)**:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/Nano/SelfContained
dotnet publish ConsoleApp.csproj -c release -r win-x64 --self-contained
docker image build -t nano-dotnet-consoleapp:2 -f Dockerfile .
docker container run -dit --name nano-dotnet-consoleapp-2 nano-dotnet-consoleapp:2
```

# Minimal WebApi

# Windows Service

# ASP.NET

# Blazor

# RabbitMQ
