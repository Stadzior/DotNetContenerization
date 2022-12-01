#Cleanup
docker container rm -f $(docker container ls -aqf label=case=queue)
docker image rm $(docker image ls -aqf label=case=stats)

#Emulate a situation where we've got available images for each element (e.g. provided by nuget feed)
docker image build -t incoming.watchdog -f ./Incoming.Watchdog/Dockerfile ./Incoming.Watchdog
docker image build -t incoming.producer -f ./Incoming.Producer/Dockerfile ./Incoming.Producer
docker image build -t incoming.consumer -f ./Incoming.Consumer/Dockerfile ./Incoming.Consumer
docker image build -t database -f ./Database/Dockerfile ./Database

docker compose up -d