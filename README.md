# DotNetContenerization
Repo for traning purposes illustrating containerization of various types of dotnet apps.

# Console App
Simple console app which prints to console "Running!" every 100ms.

### Created images:

- Alpine linux with dotnet & powershell **1.05GB**:
```
cd [PATH_TO_REPO]/ConsoleApp/Linux/Alpine/DotNetAndPowerShell
docker image build -t alpine-dotnet-consoleapp:1 -f Dockerfile .
docker container run -dit --name alpine-dotnet-consoleapp-1 alpine-dotnet-consoleapp:1
```
- Alpine linux with self-contained dotnetapp (dotnet must be installed in host) **679MB**:
```
cd [PATH_TO_REPO]/ConsoleApp/Linux/Alpine/SelfContained
dotnet publish ConsoleApp.csproj -c release -r linux-x64 --self-contained
docker image build -t alpine-dotnet-consoleapp:2 -f Dockerfile .
docker container run -dit --name alpine-dotnet-consoleapp-2 alpine-dotnet-consoleapp:2
```
- Alpine linux slim **25.9MB**:
```
cd [PATH_TO_REPO]/ConsoleApp/Linux/Alpine/Slim
docker image build -t alpine-dotnet-consoleapp:3 -f Dockerfile .
docker container run -dit --name alpine-dotnet-consoleapp-3 alpine-dotnet-consoleapp:3
```
- Windows Nano with dotnet & powershell **0B(!?)**:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/Nano/DotNetAndPowerShell
docker image build -t nano-dotnet-consoleapp:1 -f Dockerfile .
docker container run -dit --name nano-dotnet-consoleapp-1 nano-dotnet-consoleapp:1
```
- Windows Nano with self-contained dotnetapp (dotnet must be installed in host) **0B(!?)**:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/Nano/SelfContained
dotnet publish ConsoleApp.csproj -c release -r win-x64 --self-contained
docker image build -t nano-dotnet-consoleapp:2 -f Dockerfile .
docker container run -dit --name nano-dotnet-consoleapp-2 nano-dotnet-consoleapp:2
```
- Windows Nano slim **0B(!?)**:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/Nano/Slim
docker image build -t nano-dotnet-consoleapp:3 -f Dockerfile .
docker container run -dit --name nano-dotnet-consoleapp-3 nano-dotnet-consoleapp:3
```
- Windows Server Core with dotnet & powershell **0B(!?)**:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/ServerCore/DotNetAndPowerShell
docker image build -t core-dotnet-consoleapp:1 -f Dockerfile .
docker container run -dit --name core-dotnet-consoleapp-1 core-dotnet-consoleapp:1
```
- Windows Server Core with self-contained dotnetapp (dotnet must be installed in host) **0B(!?)**:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/ServerCore/SelfContained
dotnet publish ConsoleApp.csproj -c release -r win-x64 --self-contained
docker image build -t core-dotnet-consoleapp:2 -f Dockerfile .
docker container run -dit --name core-dotnet-consoleapp-2 core-dotnet-consoleapp:2
```
- Windows Server Core slim **0B(!?)**:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/ServerCore/Slim
docker image build -t core-dotnet-consoleapp:3 -f Dockerfile .
docker container run -dit --name core-dotnet-consoleapp-3 core-dotnet-consoleapp:3
```

# Minimal WebApi
Simple minimal web api which responses to simple GET request with "Running!" string.
Test if it runs by sending a GET request on `localhost:5001`

### Created images:

- Alpine linux with dotnet & powershell **1.05GB**:
```
cd [PATH_TO_REPO]/MinimalApi/Linux/Alpine/DotNetAndPowerShell
docker image build -t alpine-dotnet-minimalapi:1 -f Dockerfile .
docker container run -dit -p 5001:5000 --name alpine-dotnet-minimalapi-1 alpine-dotnet-minimalapi:1
```
- Alpine linux with self-contained dotnetapp (dotnet must be installed in host) **700MB**:
```
cd [PATH_TO_REPO]/MinimalApi/Linux/Alpine/SelfContained
dotnet publish MinimalApi.csproj -c release -r linux-x64 --self-contained
docker image build -t alpine-dotnet-minimalapi:2 -f Dockerfile .
docker container run -dit -p 5001:5000 --name alpine-dotnet-minimalapi-2 alpine-dotnet-minimalapi:2
```
- Alpine linux slim **268.5MB**:
```
cd [PATH_TO_REPO]/MinimalApi/Linux/Alpine/Slim
docker image build -t alpine-dotnet-minimalapi:3 -f Dockerfile .
docker container run -dit -p 5001:80 --name alpine-dotnet-minimalapi-3 alpine-dotnet-minimalapi:3
```
- Windows Nano with dotnet & powershell **0B(!?)**:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/Nano/DotNetAndPowerShell
docker image build -t nano-dotnet-minimalapi:1 -f Dockerfile .
docker container run -dit -p 5001:5000 --name nano-dotnet-minimalapi-1 nano-dotnet-minimalapi:1
```
- Windows Nano with self-contained dotnetapp (dotnet must be installed in host) **0B(!?)**:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/Nano/SelfContained
dotnet publish MinimalApi.csproj -c release -r win-x64 --self-contained
docker image build -t nano-dotnet-minimalapi:2 -f Dockerfile .
docker container run -dit -p 5001:5000 --name nano-dotnet-minimalapi-2 nano-dotnet-minimalapi:2
```
- Windows Nano slim **0B(!?)**:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/Nano/Slim
docker image build -t nano-dotnet-minimalapi:3 -f Dockerfile .
docker container run -dit -p 5001:80 --name nano-dotnet-minimalapi-3 nano-dotnet-minimalapi:3
```
- Windows Server Core with dotnet & powershell **0B(!?)**:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/ServerCore/DotNetAndPowerShell
docker image build -t core-dotnet-minimalapi:1 -f Dockerfile .
docker container run -dit -p 5001:5000 --name core-dotnet-minimalapi-1 core-dotnet-minimalapi:1
```
- Windows Server Core with self-contained dotnetapp (dotnet must be installed in host) **0B(!?)**:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/ServerCore/SelfContained
dotnet publish MinimalApi.csproj -c release -r win-x64 --self-contained
docker image build -t core-dotnet-minimalapi:2 -f Dockerfile .
docker container run -dit -p 5001:5000 --name core-dotnet-minimalapi-2 core-dotnet-minimalapi:2
```
- Windows Server Core slim **0B(!?)**:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/ServerCore/Slim
docker image build -t core-dotnet-minimalapi:3 -f Dockerfile .
docker container run -dit -p 5001:5000 --name core-dotnet-minimalapi-3 core-dotnet-minimalapi:3
```

# Windows Service

# Blazor
