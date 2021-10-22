package ca.bc.gov.tno.kafka;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for Kafka.
 */
@Configuration
public class KafkaConfig {
  @Value("${kafka.bootstrap.servers}")
  private String bootstrapServers;
}
