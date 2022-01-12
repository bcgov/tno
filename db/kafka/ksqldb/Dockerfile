FROM confluentinc/cp-ksqldb-server:7.0.1

# Fix Openshift logging permission issue
USER root

RUN chgrp -R root /etc/ksqldb-server && \
  chmod -R g=u /etc/ksqldb-server

RUN chown -R appuser:root /etc/ksqldb-server

USER appuser
