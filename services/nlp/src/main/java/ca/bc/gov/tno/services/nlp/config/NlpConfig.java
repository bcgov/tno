package ca.bc.gov.tno.services.nlp.config;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for Kafka.
 */
@Configuration
public class NlpConfig {
  /**
   * Kafka broker server list domain:ip (comma-delimited list).
   */
  @Value("${kafka.bootstrap-servers}")
  private String bootstrapServers;

  /**
   * Kafka consumer group ID.
   */
  @Value("${kafka.consumer.group-id}")
  private String groupId;

  /**
   * Consumer Kafka topic names that will be subscribed to (comma-delimited list).
   */
  @Value("${kafka.consumer.topics}")
  private String consumerTopics;

  /**
   * Millisecond poll timeout.
   */
  @Value("${kafka.consumer.poll-timeout:5000}")
  private int pollTimeout;

  /**
   * Kafka consumer configuration.
   */
  @Value("${enable-auto-commit:false}")
  private boolean enableAutoCommit;

  /**
   * Kafka consumer configuration.
   */
  @Value("${auto-offset-reset:earliest}")
  private String autoOffsetRest;

  /**
   * Unique client id to identify the producer.
   */
  @Value("${kafka.producer.client-id}")
  private String clientId;

  /**
   * Producer Kafka topic name that content will be pushed to.
   */
  @Value("${kafka.producer.topic}")
  private String producerTopic;

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
   * @return String return the consumerTopics
   */
  public String getconsumerTopics() {
    return consumerTopics;
  }

  /**
   * @param consumerTopics the consumerTopics to set
   */
  public void setconsumerTopics(String consumerTopics) {
    this.consumerTopics = consumerTopics;
  }

  /**
   * @return String return the producerTopic
   */
  public String getproducerTopic() {
    return producerTopic;
  }

  /**
   * @param producerTopic the producerTopic to set
   */
  public void setproducerTopic(String producerTopic) {
    this.producerTopic = producerTopic;
  }

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
   * @return String return the producerTopic
   */
  public String getProducerTopic() {
    return producerTopic;
  }

  /**
   * @param producerTopic the producerTopic to set
   */
  public void setProducerTopic(String producerTopic) {
    this.producerTopic = producerTopic;
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
