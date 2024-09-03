FROM alpine:3.17.1

RUN apk update && \
    apk add --no-cache bash bash-doc bash-completion curl tar gzip

COPY ./entrypoint.sh /scripts/entrypoint.sh
RUN chmod +x /scripts/entrypoint.sh