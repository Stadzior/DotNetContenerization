docker exec joinmq rabbitmqctl.bat stop_app
docker exec joinmq rabbitmqctl.bat join_cluster rabbit@rabbitmq
docker exec joinmq rabbitmqctl.bat start_app
