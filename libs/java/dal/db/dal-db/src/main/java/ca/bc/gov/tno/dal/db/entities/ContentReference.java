package ca.bc.gov.tno.dal.db.entities;

import java.util.Date;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.IdClass;
import javax.persistence.Table;
import javax.persistence.Temporal;
import javax.persistence.TemporalType;

import ca.bc.gov.tno.dal.db.AuditColumns;
import ca.bc.gov.tno.dal.db.KafkaMessageStatus;

/**
 * ContentReference class, provides a way to capture a reference to content from
 * data sources. This is used to map content in Kafka and to ensure duplicates
 * are not entered.
 */
@Entity
@IdClass(ContentReferencePK.class)
@Table(name = "\"ContentReference\"")
public class ContentReference extends AuditColumns {
  /**
   * The data source abbreviation.
   */
  @Id
  @Column(name = "\"source\"", nullable = false)
  private String source;

  /**
   * The source content unique key.
   */
  @Id
  @Column(name = "\"uid\"", nullable = false)
  private String uid;

  /**
   * The Kafka topic the message is stored in.
   */
  @Column(name = "\"topic\"", nullable = false)
  private String topic;

  /**
   * The Kafka partition the message is stored in.
   */
  @Column(name = "\"partition\"", nullable = false)
  private int partition;

  /**
   * The Kafka offset the message was saved.
   */
  @Column(name = "\"offset\"", nullable = false)
  private long offset;

  /**
   * The date and time the content was published.
   */
  @Temporal(TemporalType.TIMESTAMP)
  @Column(name = "\"publishedOn\"")
  private Date publishedOn;

  /**
   * The date and time the source content was updated.
   */
  @Temporal(TemporalType.TIMESTAMP)
  @Column(name = "\"sourceUpdatedOn\"")
  private Date sourceUpdatedOn;

  /**
   * The status of the reference in Kafka.
   */
  @Column(name = "\"status\"", nullable = false)
  private KafkaMessageStatus status;

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
    this.status = KafkaMessageStatus.InProgress;
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
   * @return Date return the publishedOn
   */
  public Date getPublishedOn() {
    return publishedOn;
  }

  /**
   * @param publishedOn the publishedOn to set
   */
  public void setPublishedOn(Date publishedOn) {
    this.publishedOn = publishedOn;
  }

  /**
   * @return Date return the sourceUpdatedOn
   */
  public Date getSourceUpdatedOn() {
    return sourceUpdatedOn;
  }

  /**
   * @param sourceUpdatedOn the sourceUpdatedOn to set
   */
  public void setSourceUpdatedOn(Date sourceUpdatedOn) {
    this.sourceUpdatedOn = sourceUpdatedOn;
  }

  /**
   * @return KafkaMessageStatus return the status
   */
  public KafkaMessageStatus getStatus() {
    return status;
  }

  /**
   * @param status the status to set
   */
  public void setStatus(KafkaMessageStatus status) {
    this.status = status;
  }

}
