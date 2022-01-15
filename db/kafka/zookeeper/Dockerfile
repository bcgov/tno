FROM confluentinc/cp-zookeeper:7.0.1

# Fix Openshift logging permission issue
USER root

RUN chgrp -R root /var/log/kafka && \
  chmod -R g=u /var/log/kafka

RUN chown -R appuser:root /etc/kafka /var/log/kafka /var/log/confluent /var/lib/kafka /var/lib/zookeeper /etc/zookeeper/secrets

USER appuser
