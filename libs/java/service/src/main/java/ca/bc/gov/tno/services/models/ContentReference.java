package ca.bc.gov.tno.services.models;

import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.List;

import com.fasterxml.jackson.annotation.JsonFormat;
import com.fasterxml.jackson.databind.annotation.JsonDeserialize;

import ca.bc.gov.tno.services.converters.Settings;
import ca.bc.gov.tno.services.converters.ZonedDateTimeDeserializer;

/**
 * ContentReference class, provides a way to capture a reference to content from
 * data sources. This is used to map content in Kafka and to ensure duplicates
 * are not entered.
 */
public class ContentReference extends AuditColumns {
  /**
   * The data source abbreviation.
   */
  private String source;

  /**
   * The source content unique key.
   */
  private String uid;

  /**
   * The Kafka topic the message is stored in.
   */
  private String topic;

  /**
   * The Kafka partition the message is stored in.
   */
  private int partition;

  /**
   * The Kafka offset the message was saved.
   */
  private long offset;

  /**
   * The date and time the content was published.
   */
  // @JsonFormat(shape = JsonFormat.Shape.STRING, pattern =
  // Settings.dateTimeFormat)
  // @JsonDeserialize(using = ZonedDateTimeDeserializer.class)
  private String publishedOn;

  /**
   * The date and time the source content was updated.
   */
  // @JsonFormat(shape = JsonFormat.Shape.STRING, pattern =
  // Settings.dateTimeFormat)
  // @JsonDeserialize(using = ZonedDateTimeDeserializer.class)
  private String sourceUpdatedOn;

  /**
   * The status of the reference in Kafka.
   */
  private WorkflowStatus workflowStatus;

  /**
   * A collection of content reference logs linked to this content.
   */
  private List<ContentReferenceLog> logs = new ArrayList<>();

  /**
   * Creates a new instance of a ContentReference object.
   */
  public ContentReference() {

  }

  /**
   * Creates a new instance of a ContentReference object, initializes with
   * specified parameters.
   * 
   * @param source Data source abbreviation
   * @param uid    Unique identify of the content
   * @param topic  The Kafka topic it was stored in
   */
  public ContentReference(String source, String uid, String topic) {
    this.source = source;
    this.uid = uid;
    this.topic = topic;
    this.partition = -1;
    this.offset = -1;
    this.workflowStatus = WorkflowStatus.InProgress;
  }

  /**
   * Creates a new instance of a ContentReference object, initializes with
   * specified parameters.
   * 
   * @param source  Data source abbreviation
   * @param uid     Unique identify of the content
   * @param topic   The Kafka topic it was stored in
   * @param version Row version value
   */
  public ContentReference(String source, String uid, String topic, long version) {
    this(source, uid, topic);
    this.setVersion(version);
  }

  /**
   * @return String return the source
   */
  public String getSource() {
    return source;
  }

  /**
   * @param source the source to set
   */
  public void setSource(String source) {
    this.source = source;
  }

  /**
   * @return String return the uid
   */
  public String getUid() {
    return uid;
  }

  /**
   * @param uid the uid to set
   */
  public void setUid(String uid) {
    this.uid = uid;
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
   * @return ZonedDateTime return the publishedOn
   */
  public String getPublishedOn() {
    return publishedOn;
  }

  /**
   * @param publishedOn the publishedOn to set
   */
  public void setPublishedOn(String publishedOn) {
    this.publishedOn = publishedOn;
  }

  /**
   * @return ZonedDateTime return the sourceUpdatedOn
   */
  public String getSourceUpdatedOn() {
    return sourceUpdatedOn;
  }

  /**
   * @param sourceUpdatedOn the sourceUpdatedOn to set
   */
  public void setSourceUpdatedOn(String sourceUpdatedOn) {
    this.sourceUpdatedOn = sourceUpdatedOn;
  }

  /**
   * @return WorkflowStatus return the workflowStatus
   */
  public WorkflowStatus getWorkflowStatus() {
    return workflowStatus;
  }

  /**
   * @param workflowStatus the workflowStatus to set
   */
  public void setWorkflowStatus(WorkflowStatus workflowStatus) {
    this.workflowStatus = workflowStatus;
  }

  /**
   * @return List{ContentReferenceLog} return the logs
   */
  public List<ContentReferenceLog> getLogs() {
    return logs;
  }

  /**
   * @param logs the logs to set
   */
  public void setLogs(List<ContentReferenceLog> logs) {
    this.logs = logs;
  }

}
