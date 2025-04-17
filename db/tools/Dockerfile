FROM alpine/psql:latest

RUN apk update && apk add -y --no-cache iputils-ping bind-tools wget

ENTRYPOINT ["tail", "-f", "/dev/null"]
