# Force remove containers
docker container rm -f $(docker container ls -aqf label=case=stats)

# Remove images
docker image rm $(docker image ls -aqf label=case=stats)
docker image rm $(docker image ls -aqf "dangling=true")
docker image rm $(docker image ls -aq mcr.microsoft.com/dotnet/sdk)
docker image rm $(docker image ls -aq mcr.microsoft.com/dotnet/aspnet)
docker image rm $(docker image ls -aq mcr.microsoft.com/windows/servercore)