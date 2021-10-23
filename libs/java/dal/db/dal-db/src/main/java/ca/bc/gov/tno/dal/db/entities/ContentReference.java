package ca.bc.gov.tno.dal.db.entities;

import java.util.Date;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.IdClass;
import javax.persistence.Table;
import javax.persistence.Temporal;
import javax.persistence.TemporalType;

import ca.bc.gov.tno.dal.db.KafkaMessageStatus;

/**
 * DataSourceReference class, provides a way to capture a reference to content
 * from data sources. This is used to map content in Kafka and to ensure
 * duplicates are not entered.
 */
@Entity
@IdClass(ContentReferencePK.class)
@Table(name = "\"DataSourceReference\"")
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
   * The Kafka offset the message was saved.
   */
  @Column(name = "\"offset\"", nullable = false)
  private int offset;

  /**
   * The date and time the content was published.
   */
  @Temporal(TemporalType.TIMESTAMP)
  @Column(name = "\"publishedOn\"")
  private Date publishedOn;

  /**
   * The status of the reference in Kafka.
   */
  @Column(name = "\"status\"", nullable = false)
  private KafkaMessageStatus status;

  /**
   * Creates a new instance of a DataSourceReference object.
   */
  public ContentReference() {

  }

  /**
   * Creates a new instance of a DataSourceReference object, initializes with
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
   * @return int return the offset
   */
  public int getOffset() {
    return offset;
  }

  /**
   * @param offset the offset to set
   */
  public void setOffset(int offset) {
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
