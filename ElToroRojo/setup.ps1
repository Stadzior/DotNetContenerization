docker container rm -f $(docker container ls -aqf label=case=queue)
docker image rm $(docker image ls -aqf label=case=stats)

docker image build -t incoming.watchdog -f ./Incoming.Watchdog/Dockerfile ./Incoming.Watchdog
docker image build -t incoming.producer -f ./Incoming.Producer/Dockerfile ./Incoming.Producer

docker compose up -d