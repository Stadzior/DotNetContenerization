# https://github.com/dotnet/dotnet-docker/blob/main/samples/dotnetapp/Dockerfile.nanoserver-x64-slim
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=Blazor

WORKDIR /source
    
# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore -r win-x64 /p:PublishReadyToRun=true

# copy and publish app and libraries
COPY . .
RUN dotnet publish -c Release -o /app -r win-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.10-nanoserver-1809

ENV \
    # Ensure we listen on any IP Address 
    DOTNET_URLS=http://+:80

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=Blazor

WORKDIR /app
COPY --from=build /source/bin/Release/net6.0 .

# Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["Blazor.exe"]