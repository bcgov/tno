FROM docker.elastic.co/elasticsearch/elasticsearch:8.19.21

COPY ./elasticsearch.yml /usr/share/elasticsearch/config/elasticsearch.yml

# Snapshot repository (path.repo). A new docker volume copies this directory's ownership, so Elasticsearch can write to it.
USER root
RUN mkdir -p /usr/share/elasticsearch/snapshots && \
    chown elasticsearch:root /usr/share/elasticsearch/snapshots && \
    chmod 0775 /usr/share/elasticsearch/snapshots
USER elasticsearch

EXPOSE 9200 9300
