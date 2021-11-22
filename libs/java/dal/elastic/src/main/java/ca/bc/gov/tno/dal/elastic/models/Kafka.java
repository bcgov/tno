package ca.bc.gov.tno.dal.elastic.models;

import org.springframework.data.elasticsearch.annotations.Field;
import org.springframework.data.elasticsearch.annotations.FieldType;

/**
 * Kafka class, provides a model to capture Kafka metadata.
 */
public class Kafka {

  /**
   * The Kafka topic that the content is stored in.
   */
  @Field(type = FieldType.Keyword, index = false)
  private String topic;

  /**
   * The Kafka offset in the topic where the message is stored.
   */
  @Field(type = FieldType.Long, index = false)
  private long offset;

  /**
   * The Kafka partition where the message is stored.
   */
  @Field(type = FieldType.Integer, index = false)
  private int partition;

  /**
   * Creates a new instance of a Kafka object.
   */
  public Kafka() {
  }

  /**
   * Creates a new instance of a Kafka object, initializes with specified
   * parameters.
   * 
   * @param topic     The Kafka topic the message is stored in.
   * @param partition The Kafka partition the message is stored in.
   * @param offset    The Kafka topic offset the message is stored at.
   */
  public Kafka(final String topic, final int partition, final long offset) {
    this.topic = topic;
    this.partition = partition;
    this.offset = offset;
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

  /**
   * @return long return the offset
   */
  public long getOffset() {
    return offset;
  }

  /**
   * @param offset the offset to set
   */
  public void setOffset(long offset) {
    this.offset = offset;
  }

  /**
   * @return int return the partition
   */
  public int getPartition() {
    return partition;
  }

  /**
   * @param partition the partition to set
   */
  public void setPartition(int partition) {
    this.partition = partition;
  }

}
