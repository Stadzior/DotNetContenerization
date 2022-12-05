# https://github.com/dotnet/dotnet-docker/blob/main/samples/dotnetapp/Dockerfile.alpine-x64-slim
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

LABEL purpose=build
LABEL case=queue

WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore -r linux-musl-x64 /p:PublishReadyToRun=true

# copy and publish app and libraries
COPY . .
RUN dotnet publish -c Release -o /app -r linux-musl-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishSingleFile=true

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.11-alpine3.16-amd64

LABEL purpose=consumer
LABEL case=queue
LABEL apptype=consoleapp

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ./Incoming.Consumer ${FILE_NAME}