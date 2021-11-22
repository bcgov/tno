package ca.bc.gov.tno.services.kafka.config;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;

/**
 * KafkaConfig class, provides configuration settings for the Kafka Cluster.
 */
@Configuration
public class KafkaConfig {
  /**
   * Kafka broker server list domain:ip (comma-delimited list).
   */
  @Value("${kafka.bootstrap-servers}")
  private String bootstrapServers;

  /**
   * @return String return the bootstrapServers
   */
  public String getBootstrapServers() {
    return bootstrapServers;
  }

  /**
   * @param bootstrapServers the bootstrapServers to set
   */
  public void setBootstrapServers(String bootstrapServers) {
    this.bootstrapServers = bootstrapServers;
  }

}
