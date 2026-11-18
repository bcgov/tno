FROM bitnami/git:latest

COPY entrypoint.sh /scripts/entrypoint.sh
RUN chmod +x /scripts/entrypoint.sh

# This volume is the local storage for capture A/V files.
RUN mkdir /data
VOLUME /data
RUN chgrp -R 0 /data && \
    chmod -R g=u /data && \
    chown -R 1001 /data

RUN echo "test 2"

USER 1001
# USER 1010220000
ENTRYPOINT ["tail", "-f", "/dev/null"]
# ENTRYPOINT ["/scripts/entrypoint.sh"]
