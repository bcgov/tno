FROM confluentinc/cp-kafka:7.0.1

# Fix Openshift logging permission issue
USER root

RUN chgrp -R root /var/log/kafka && \
  chmod -R g=u /var/log/kafka

RUN chown -R appuser:root /var/log/kafka /var/log/confluent

USER appuser
