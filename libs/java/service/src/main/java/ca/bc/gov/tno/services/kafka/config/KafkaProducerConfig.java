package ca.bc.gov.tno.services.kafka.config;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for Kafka producer.
 */
@Configuration
public class KafkaProducerConfig extends KafkaConfig {
  /**
   * Unique client id to identify the producer.
   */
  @Value("${kafka.producer.client-id:}")
  private String clientId;

  /**
   * The topic to send messages to.
   */
  @Value("${kafka.producer.topic:")
  private String topic;

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

  /**
   * @return String return the topic
   */
  public String getTopic() {
    return topic;
  }

  /**
   * @param topic the topic to set
   */
  public void setTopic(String topic) {
    this.topic = topic;
  }

}
