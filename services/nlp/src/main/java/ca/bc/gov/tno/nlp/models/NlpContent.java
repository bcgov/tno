package ca.bc.gov.tno.nlp.models;

import java.util.stream.Collectors;

/**
 * ContentStub class, provides a model for capturing all the relevant data that
 * will make content searchable.
 */
public class NlpContent extends ContentBase {

  /**
   * The Kafka topic that the content is stored in.
   */
  private String topic;

  /**
   * The Kafka offset in the topic where the content is stored.
   */
  private long offset;

  /**
   * Creates a new instance of a ContentStub object.
   */
  public NlpContent() {
  }

  /**
   * Creates a new instance of a ContentStub object, initializes with specified
   * parameters.
   * 
   * @param source
   * @param topic
   * @param offset
   * @param uid
   */
  public NlpContent(String source, ContentType type, String uid, String title, String topic, long offset) {
    super(source, type, uid, title);

    if (topic == null || topic.length() == 0)
      throw new IllegalArgumentException("Parameter 'topic' is required, and cannot be empty.");
    if (offset < 0)
      throw new IllegalArgumentException("Parameter 'offset' cannot be less than 0.");

    this.topic = topic;
    this.offset = offset;
  }

  public NlpContent(ContentBase content) {
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

}
