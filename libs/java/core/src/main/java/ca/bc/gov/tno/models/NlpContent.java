package ca.bc.gov.tno.models;

import java.util.stream.Collectors;

import ca.bc.gov.tno.ContentType;

/**
 * NlpContent class, provides a model for capturing all the relevant data that
 * can be indexed for search.
 */
public class NlpContent extends ContentBase {

  /**
   * The Kafka topic that the content is stored in.
   */
  private String topic;

  /**
   * The Kafka offset in the topic where the message is stored.
   */
  private long offset;

  /**
   * The Kafka partition where the message is stored.
   */
  private int partition;

  /**
   * Creates a new instance of a NlpContent object.
   */
  public NlpContent() {
  }

  /**
   * Creates a new instance of a NlpContent object, initializes with specified
   * parameters.
   * 
   * @param source    The unique identifier of the source (i.e. code).
   * @param type      The type of content [text|audio|video].
   * @param uid       A unique identifier for the content.
   * @param title     The content title.
   * @param topic     The Kafka topic the message is stored in.
   * @param partition The Kafka partition the message is stored in.
   * @param offset    The Kafka offset the message is stored at.
   */
  public NlpContent(String source, ContentType type, String uid, String title, String topic, int partition,
      long offset) {
    super(source, type, uid, title);

    if (topic == null || topic.length() == 0)
      throw new IllegalArgumentException("Parameter 'topic' is required, and cannot be empty.");
    if (partition < 0)
      throw new IllegalArgumentException("Parameter 'partition' cannot be less than 0.");
    if (offset < 0)
      throw new IllegalArgumentException("Parameter 'offset' cannot be less than 0.");

    this.topic = topic;
    this.partition = partition;
    this.offset = offset;
  }

  /**
   * Creates a new instance of a NlpContent object, initializes with specified
   * parameters.
   * 
   * @param content   The original source content data.
   * @param topic     The Kafka topic the message is stored in.
   * @param partition The Kafka partition the message is stored in.
   * @param offset    The Kafka offset the message is stored at.
   */
  public NlpContent(ContentBase content, String topic, int partition, long offset) {
    if (content == null)
      throw new IllegalArgumentException("Parameter 'content' is required.");

    this.setSource(content.getSource());
    this.setType(content.getType());
    this.setUid(content.getUid());
    this.setLink(content.getLink());
    this.setLanguage(content.getLanguage());
    this.setAuthor(content.getAuthor());
    this.setTitle(content.getTitle());
    this.setSummary(content.getSummary());
    this.setFilePath(content.getFilePath());
    this.setStreamUrl(content.getStreamUrl());
    this.setPublishedOn(content.getPublishedOn());
    this.setUpdatedOn(content.getUpdatedOn());
    this.setTags(content.getTags().stream().map(t -> new Tag(t.getKey(), t.getValue())).collect(Collectors.toList()));

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
