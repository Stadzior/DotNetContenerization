# DotNetContenerization
Repo for traning purposes illustrating contenerization of various types of dotnet apps.

# Console App
Simple console app which prints to console "Running!" every 100ms.
Created images:
- Alpine linux with dotnet & powershell **1.05GB**:
```
cd [PATH_TO_REPO]/ConsoleApp
docker image build -t alpine-dotnet-consoleapp -f Linux/Alpine/DotNetAndPowerShell/Dockerfile .
docker container run -dit --name alpine-dotnet-consoleapp-1 alpine-dotnet-consoleapp
```
- Alpine linux with self-contained dotnetapp **967MB** :
```
cd [PATH_TO_REPO]/ConsoleApp
docker image build -t alpine-dotnet-consoleapp:2 -f Linux/Alpine/SelfContained/Dockerfile .
docker container run -dit --name alpine-dotnet-consoleapp-2 alpine-dotnet-consoleapp:2
```
- Windows Nano with dotnet & powershell **xD**:
```
cd [PATH_TO_REPO]/ConsoleApp
docker image build -t nano-dotnet-consoleapp -f Windows/Nano/DotNetAndPowerShell/Dockerfile .
docker container run -dit --name nano-dotnet-consoleapp-1 nano-dotnet-consoleapp
```

# Minimal WebApi

# Windows Service

# ASP.NET

# Blazor

# RabbitMQ
