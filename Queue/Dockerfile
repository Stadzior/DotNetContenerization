FROM rabbitmq:3.11-management

LABEL purpose=workshops
LABEL case=stats
LABEL apptype=Queue

WORKDIR usr/setup
COPY . .

ENTRYPOINT ["./entrypoint.sh"]

EXPOSE 5673 15673