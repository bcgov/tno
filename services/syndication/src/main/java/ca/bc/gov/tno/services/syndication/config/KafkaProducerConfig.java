package ca.bc.gov.tno.services.syndication.config;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for Kafka producer.
 */
@Configuration
public class KafkaProducerConfig {

  /**
   * Kafka broker server list domain:ip (comma-delimited list).
   */
  @Value("${kafka.bootstrap-servers}")
  private String bootstrapServers;

  /**
   * Unique client id to identify the producer.
   */
  @Value("${kafka.producer.client-id}")
  private String clientId;

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

  /**
   * @return String return the clientId
   */
  public String getClientId() {
    return clientId;
  }

  /**
   * @param clientId the clientId to set
   */
  public void setClientId(String clientId) {
    this.clientId = clientId;
  }

}
