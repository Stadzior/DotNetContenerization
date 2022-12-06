docker image pull rabbitmq:3.11-management 
docker image build -t incoming.producer -f ./Incoming.Watchdog/Incoming.Producer/Dockerfile ./Incoming.Watchdog/Incoming.Producer
docker image build -t incoming.consumer -f ./Incoming.Watchdog/Incoming.Consumer/Dockerfile ./Incoming.Watchdog/Incoming.Consumer
docker image build -t incoming.api -f ./Incoming.Watchdog/Incoming.Api/Dockerfile ./Incoming.Watchdog/Incoming.Api