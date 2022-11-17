# https://github.com/dotnet/dotnet-docker/blob/main/samples/dotnetapp/Dockerfile.nanoserver-x64-slim
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=Blazor

WORKDIR /source
    
# copy and publish app and libraries
COPY . .
RUN dotnet publish -c Release -o /app --self-contained /p:PublishTrimmed=true

FROM nginx:alpine AS final

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=Blazor

WORKDIR /usr/share/nginx/html
COPY --from=build /app/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf