FROM mcr.microsoft.com/windows/servercore:20H2

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=WindowsService
    
# Copy files
WORKDIR /app
COPY bin/release/net6.0/win-x64/publish .

# Run app
ENTRYPOINT ["C:/app/WindowsService.exe"]