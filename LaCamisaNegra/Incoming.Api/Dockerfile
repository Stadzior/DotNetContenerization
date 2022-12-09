# https://github.com/dotnet/dotnet-docker/blob/main/samples/dotnetapp/Dockerfile.nanoserver-x64-slim
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

LABEL purpose=build
LABEL case=lacamisanegra

WORKDIR /source

ENV \
    # Ensure we listen on any IP Address 
    DOTNET_URLS=http://+:80
    
# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore -r win-x64 /p:PublishReadyToRun=true

# copy and publish app and libraries
COPY . .
RUN dotnet publish -c Release -o /app -r win-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishSingleFile=true

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.10-nanoserver-1809

LABEL purpose=api
LABEL case=lacamisanegra
LABEL apptype=api

WORKDIR /app
COPY --from=build /app .

# Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["Incoming.Api.exe"]