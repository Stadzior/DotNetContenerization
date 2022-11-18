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

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.10-nanoserver-1809

ENV \
    # Ensure we listen on any IP Address 
    DOTNET_URLS=http://+:80

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=Blazor

WORKDIR /app
COPY --from=build /app/wwwroot .
#config file COPY nginx.conf /etc/nginx/nginx.conf

# Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true

#ENTRYPOINT ["Blazor"]