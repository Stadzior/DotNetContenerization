FROM mcr.microsoft.com/windows/servercore:20H2

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=ConsoleApp

# Copy files
COPY bin/release/net6.0/win-x64/publish C:/app

# Run app
ENTRYPOINT ["C:/app/ConsoleApp.exe"]