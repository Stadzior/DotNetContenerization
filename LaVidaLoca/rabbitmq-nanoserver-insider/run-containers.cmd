docker run -d --hostname rabbitmq --name rabbitmq --mount type=bind,source="%cd%\data.runtime",target=c:\data rabbitmq:nanoserver-insider
docker run -d --hostname joinmq --name joinmq --mount type=bind,source="%cd%\data.runtime",target=c:\data rabbitmq:nanoserver-insider
