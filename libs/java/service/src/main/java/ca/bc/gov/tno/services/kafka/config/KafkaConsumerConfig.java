package ca.bc.gov.tno.services.kafka.config;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;

/**
 * KafkaConsumerConfig class, provides configuration settings for the Kafka
 * Consumer.
 */
@Configuration
public class KafkaConsumerConfig extends KafkaConfig {
  /**
   * Kafka consumer group ID.
   */
  @Value("${kafka.consumer.group-id:}")
  private String groupId;

  /**
   * Consumer Kafka topic names that will be subscribed to (comma-delimited list).
   */
  @Value("${kafka.consumer.topics:}")
  private String consumerTopics;

  /**
   * Millisecond poll timeout.
   */
  @Value("${kafka.consumer.poll-timeout:5000}")
  private int pollTimeout;

  /**
   * Kafka consumer configuration.
   */
  @Value("${kafka.consumer.enable-auto-commit:false}")
  private boolean enableAutoCommit;

  /**
   * Kafka consumer configuration.
   */
  @Value("${kafka.consumer.auto-offset-reset:earliest}")
  private String autoOffsetRest;

  /**
   * @return String return the groupId
   */
  public String getGroupId() {
    return groupId;
  }

  /**
   * @param groupId the groupId to set
   */
  public void setGroupId(String groupId) {
    this.groupId = groupId;
  }

  /**
   * @return String return the consumerTopics
   */
  public String getConsumerTopics() {
    return consumerTopics;
  }

  /**
   * @param consumerTopics the consumerTopics to set
   */
  public void setConsumerTopics(String consumerTopics) {
    this.consumerTopics = consumerTopics;
  }

  /**
   * @return int return the pollTimeout
   */
  public int getPollTimeout() {
    return pollTimeout;
  }

  /**
   * @param pollTimeout the pollTimeout to set
   */
  public void setPollTimeout(int pollTimeout) {
    this.pollTimeout = pollTimeout;
  }

  /**
   * @return boolean return the enableAutoCommit
   */
  public boolean isEnableAutoCommit() {
    return enableAutoCommit;
  }

  /**
   * @param enableAutoCommit the enableAutoCommit to set
   */
  public void setEnableAutoCommit(boolean enableAutoCommit) {
    this.enableAutoCommit = enableAutoCommit;
  }

  /**
   * @return String return the autoOffsetRest
   */
  public String getAutoOffsetRest() {
    return autoOffsetRest;
  }

  /**
   * @param autoOffsetRest the autoOffsetRest to set
   */
  public void setAutoOffsetRest(String autoOffsetRest) {
    this.autoOffsetRest = autoOffsetRest;
  }

}
