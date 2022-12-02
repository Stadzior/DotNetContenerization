#Cleanup
docker container rm -f $(docker container ls -aqf label=case=queue)
#docker image rm $(docker image ls -aqf label=case=queue)
#docker image rm rabbitmq:3.11-management

#Emulate a situation where we've got available images for each element (e.g. provided by nuget feed)
docker image pull rabbitmq:3.11-management
# docker image build -t errordatabase -f ./ErrorDatabase/Dockerfile ./ErrorDatabase
# docker image build -t targetdatabase -f ./TargetDatabase/Dockerfile ./TargetDatabase
docker image build -t incoming.watchdog -f ./Incoming.Watchdog/Dockerfile ./Incoming.Watchdog
docker image build -t incoming.producer -f ./Incoming.Producer/Dockerfile ./Incoming.Producer
docker image build -t incoming.consumer -f ./Incoming.Consumer/Dockerfile ./Incoming.Consumer
docker image build -t incoming.api -f ./Incoming.Api/Dockerfile ./Incoming.Api

docker compose up -d
docker image prune -f