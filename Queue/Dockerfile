FROM rabbitmq:3.11-management

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=Queue

RUN \
    rabbitmqadmin declare exchange name=test-exchange type=direct && \
    rabbitmqadmin declare queue name=test-queue durable=false && \
    rabbitmqadmin declare binding source="test-exchange" destination_type="queue" destination="test-queue" routing_key="test-key"

EXPOSE 5673 15673