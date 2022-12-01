# DotNetContenerization
Repo for traning purposes illustrating containerization of various types of dotnet apps.

Usage:
1. Performance/resource usage/stats comparison:
   1. Switch to either Windows or Linux containers in docker desktop.
   2. Run `stats.ps1`
   3. Switch back to the other os type.
   4. Run `stats.ps1` again. Warning! Some images e.g. dotnet server core are around ~6GB in size. Modify the script if you want to run only part of the images.
   5. Observe with `docker stats` and/or `docker ps`
   6. To cleanup run `stats_cleanup.ps1` (double check by yourself and do another cleanup if needed).
2. Queue integration example:
   1. Switch to Linux containers in docker desktop.
   2. Create `C:\workspace` folder in file explorer and add it to docker [Filesharing](https://stackoverflow.com/questions/70877785/docker-error-response-from-daemon-user-declined-directory-sharing).
   3. run `queue.ps1`
   4. Open the following to observe the whole process:
      1. Open `C:\workspace` in file explorer
      2. Open queue visualisation under `http://localhost:15672/#/queues` (l:guest, p:guest)
      3. Open frontend page under `http://localhost:5001`
      4. Open producer & consumer logs in docker desktop.
   5. Put some .txt files in `C:\workspace`
   6. Observe how containers are beeing created & removed as messages appears in frontend page.
   7. To cleanup run `queue_cleanup.ps1` (double check by yourself and do another cleanup if needed).

# Performance/resource usage/stats comparison:
## Console App
Simple console app which prints to console "Running!" every 100ms.

- Alpine linux with dotnet & powershell:
```
cd [PATH_TO_REPO]/ConsoleApp/Linux/Alpine/DotNetAndPowerShell
docker image build -t alpine-dotnet-consoleapp:1 -f Dockerfile .
docker container run -d --name alpine-dotnet-consoleapp-1 alpine-dotnet-consoleapp:1
```
- Alpine linux with self-contained dotnetapp (dotnet must be installed in host):
```
cd [PATH_TO_REPO]/ConsoleApp/Linux/Alpine/SelfContained
dotnet publish ConsoleApp.csproj -c release -r linux-x64 --self-contained
docker image build -t alpine-dotnet-consoleapp:2 -f Dockerfile .
docker container run -d --name alpine-dotnet-consoleapp-2 alpine-dotnet-consoleapp:2
```
- Alpine linux slim:
```
cd [PATH_TO_REPO]/ConsoleApp/Linux/Alpine/Slim
docker image build -t alpine-dotnet-consoleapp:3 -f Dockerfile .
docker container run -d --name alpine-dotnet-consoleapp-3 alpine-dotnet-consoleapp:3
```
- Windows Nano with dotnet & powershell:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/Nano/DotNetAndPowerShell
docker image build -t nano-dotnet-consoleapp:1 -f Dockerfile .
docker container run -d --name nano-dotnet-consoleapp-1 nano-dotnet-consoleapp:1
```
- Windows Nano with self-contained dotnetapp (dotnet must be installed in host):
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/Nano/SelfContained
dotnet publish ConsoleApp.csproj -c release -r win-x64 --self-contained
docker image build -t nano-dotnet-consoleapp:2 -f Dockerfile .
docker container run -d --name nano-dotnet-consoleapp-2 nano-dotnet-consoleapp:2
```
- Windows Nano slim:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/Nano/Slim
docker image build -t nano-dotnet-consoleapp:3 -f Dockerfile .
docker container run -d --name nano-dotnet-consoleapp-3 nano-dotnet-consoleapp:3
```
- Windows Server Core with dotnet & powershell:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/ServerCore/DotNetAndPowerShell
docker image build -t core-dotnet-consoleapp:1 -f Dockerfile .
docker container run -d --name core-dotnet-consoleapp-1 core-dotnet-consoleapp:1
```
- Windows Server Core with self-contained dotnetapp (dotnet must be installed in host):
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/ServerCore/SelfContained
dotnet publish ConsoleApp.csproj -c release -r win-x64 --self-contained
docker image build -t core-dotnet-consoleapp:2 -f Dockerfile .
docker container run -d --name core-dotnet-consoleapp-2 core-dotnet-consoleapp:2
```
- Windows Server Core slim:
```
cd [PATH_TO_REPO]/ConsoleApp/Windows/ServerCore/Slim
docker image build -t core-dotnet-consoleapp:3 -f Dockerfile .
docker container run -d --name core-dotnet-consoleapp-3 core-dotnet-consoleapp:3
```

# Minimal WebApi
Simple minimal web api which responses to simple GET request with "Running!" string.
Test if it runs by sending a GET request on `localhost:5001`

- Alpine linux with dotnet & powershell:
```
cd [PATH_TO_REPO]/MinimalApi/Linux/Alpine/DotNetAndPowerShell
docker image build -t alpine-dotnet-minimalapi:1 -f Dockerfile .
docker container run -d -p 5001:5000 --name alpine-dotnet-minimalapi-1 alpine-dotnet-minimalapi:1
```
- Alpine linux with self-contained dotnetapp (dotnet must be installed in host):
```
cd [PATH_TO_REPO]/MinimalApi/Linux/Alpine/SelfContained
dotnet publish MinimalApi.csproj -c release -r linux-x64 --self-contained
docker image build -t alpine-dotnet-minimalapi:2 -f Dockerfile .
docker container run -d -p 5001:5000 --name alpine-dotnet-minimalapi-2 alpine-dotnet-minimalapi:2
```
- Alpine linux slim:
```
cd [PATH_TO_REPO]/MinimalApi/Linux/Alpine/Slim
docker image build -t alpine-dotnet-minimalapi:3 -f Dockerfile .
docker container run -d -p 5001:80 --name alpine-dotnet-minimalapi-3 alpine-dotnet-minimalapi:3
```
- Windows Nano with dotnet & powershell:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/Nano/DotNetAndPowerShell
docker image build -t nano-dotnet-minimalapi:1 -f Dockerfile .
docker container run -d -p 5001:5000 --name nano-dotnet-minimalapi-1 nano-dotnet-minimalapi:1
```
- Windows Nano with self-contained dotnetapp (dotnet must be installed in host):
```
cd [PATH_TO_REPO]/MinimalApi/Windows/Nano/SelfContained
dotnet publish MinimalApi.csproj -c release -r win-x64 --self-contained
docker image build -t nano-dotnet-minimalapi:2 -f Dockerfile .
docker container run -d -p 5001:5000 --name nano-dotnet-minimalapi-2 nano-dotnet-minimalapi:2
```
- Windows Nano slim:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/Nano/Slim
docker image build -t nano-dotnet-minimalapi:3 -f Dockerfile .
docker container run -d -p 5001:80 --name nano-dotnet-minimalapi-3 nano-dotnet-minimalapi:3
```
- Windows Server Core with dotnet & powershell:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/ServerCore/DotNetAndPowerShell
docker image build -t core-dotnet-minimalapi:1 -f Dockerfile .
docker container run -d -p 5001:5000 --name core-dotnet-minimalapi-1 core-dotnet-minimalapi:1
```
- Windows Server Core with self-contained dotnetapp (dotnet must be installed in host):
```
cd [PATH_TO_REPO]/MinimalApi/Windows/ServerCore/SelfContained
dotnet publish MinimalApi.csproj -c release -r win-x64 --self-contained
docker image build -t core-dotnet-minimalapi:2 -f Dockerfile .
docker container run -d -p 5001:5000 --name core-dotnet-minimalapi-2 core-dotnet-minimalapi:2
```
- Windows Server Core slim:
```
cd [PATH_TO_REPO]/MinimalApi/Windows/ServerCore/Slim
docker image build -t core-dotnet-minimalapi:3 -f Dockerfile .
docker container run -d -p 5001:5000 --name core-dotnet-minimalapi-3 core-dotnet-minimalapi:3
```

# Windows Service

- Alpine linux with dotnet & powershell:
```
cd [PATH_TO_REPO]/WindowsService/Linux/Alpine/DotNetAndPowerShell
docker image build -t alpine-dotnet-windowsservice:1 -f Dockerfile .
docker container run -d --name alpine-dotnet-windowsservice-1 alpine-dotnet-windowsservice:1
```
- Alpine linux with self-contained dotnetapp (dotnet must be installed in host):
```
cd [PATH_TO_REPO]/WindowsService/Linux/Alpine/SelfContained
dotnet publish WindowsService.csproj -c release -r linux-x64 --self-contained
docker image build -t alpine-dotnet-windowsservice:2 -f Dockerfile .
docker container run -d --name alpine-dotnet-windowsservice-2 alpine-dotnet-windowsservice:2
```
- Alpine linux slim:
```
cd [PATH_TO_REPO]/WindowsService/Linux/Alpine/Slim
docker image build -t alpine-dotnet-windowsservice:3 -f Dockerfile .
docker container run -d --name alpine-dotnet-windowsservice-3 alpine-dotnet-windowsservice:3
```
- Windows Nano with dotnet & powershell:
```
cd [PATH_TO_REPO]/WindowsService/Windows/Nano/DotNetAndPowerShell
docker image build -t nano-dotnet-windowsservice:1 -f Dockerfile .
docker container run -d --name nano-dotnet-windowsservice-1 nano-dotnet-windowsservice:1
```
- Windows Nano with self-contained dotnetapp (dotnet must be installed in host):
```
cd [PATH_TO_REPO]/WindowsService/Windows/Nano/SelfContained
dotnet publish WindowsService.csproj -c release -r win-x64 --self-contained
docker image build -t nano-dotnet-windowsservice:2 -f Dockerfile .
docker container run -d --name nano-dotnet-windowsservice-2 nano-dotnet-windowsservice:2
```
- Windows Nano slim:
```
cd [PATH_TO_REPO]/WindowsService/Windows/Nano/Slim
docker image build -t nano-dotnet-windowsservice:3 -f Dockerfile .
docker container run -d--name nano-dotnet-windowsservice-3 nano-dotnet-windowsservice:3
```
- Windows Server Core with dotnet & powershell:
```
cd [PATH_TO_REPO]/WindowsService/Windows/ServerCore/DotNetAndPowerShell
docker image build -t core-dotnet-windowsservice:1 -f Dockerfile .
docker container run -d --name core-dotnet-windowsservice-1 core-dotnet-windowsservice:1
```
- Windows Server Core with self-contained dotnetapp (dotnet must be installed in host):
```
cd [PATH_TO_REPO]/WindowsService/Windows/ServerCore/SelfContained
dotnet publish WindowsService.csproj -c release -r win-x64 --self-contained
docker image build -t core-dotnet-windowsservice:2 -f Dockerfile .
docker container run -d --name core-dotnet-windowsservice-2 core-dotnet-windowsservice:2
```
- Windows Server Core slim:
```
cd [PATH_TO_REPO]/WindowsService/Windows/ServerCore/Slim
docker image build -t core-dotnet-windowsservice:3 -f Dockerfile .
docker container run -d --name core-dotnet-windowsservice-3 core-dotnet-windowsservice:3
```

# Blazor

- Alpine linux slim:
```
cd [PATH_TO_REPO]/Blazor/Linux/Alpine/Slim
docker image build -t alpine-dotnet-blazor:3 -f Dockerfile .
docker container run -d --name alpine-dotnet-blazor-3 alpine-dotnet-blazor:3
```
- Windows Nano slim:
```
cd [PATH_TO_REPO]/Blazor/Windows/Nano/Slim
docker image build -t nano-dotnet-blazor:3 -f Dockerfile .
docker container run -d --name nano-dotnet-blazor-3 nano-dotnet-blazor:3
```

# MSSQL:

- Ubuntu-based image (only available at this time):
```
cd [PATH_TO_REPO]/MSSQL/Linux/Ubuntu
docker image build -t ubuntu-dotnet-mssql:1 -f Dockerfile .
docker container run -d -p 1434:1433 --name ubuntu-dotnet-mssql-1 ubuntu-dotnet-mssql:1
```
You can now connect to your db via SSMS with the following creds:
```
Server type: Database Engine
Server name: localhost, 1434
Authentication: SQL Server Authentication
Login: sa
Password: zaq1@WSX
```
Or via cli:
`docker exec -it ubuntu-dotnet-mssql-1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P zaq1@WSX`

# Queue:
```
docker container run -d -p 1434:1433 --name rabbitmq rabbitmq:3.11-management
docker exec rabbitmq rabbitmqadmin declare exchange name=test-exchange type=direct
docker exec rabbitmq rabbitmqadmin declare queue name=test-queue durable=false
docker exec rabbitmq rabbitmqadmin declare binding source="test-exchange" destination_type="queue" destination="test-queue" routing_key="test-key"
```
