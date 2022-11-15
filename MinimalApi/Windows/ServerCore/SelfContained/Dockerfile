FROM mcr.microsoft.com/windows/servercore:20H2

LABEL purpose=workshops
LABEL apptype=MinimalApi

ENV \
    # Unset ASPNETCORE_URLS from aspnet base image
    ASPNETCORE_URLS= \
    # Ensure we listen on any IP Address 
    DOTNET_URLS=http://+:5000
    
# Copy files
WORKDIR /app
COPY bin/release/net6.0/win-x64/publish .

# Run app
ENTRYPOINT ["C:/app/MinimalApi.exe"]