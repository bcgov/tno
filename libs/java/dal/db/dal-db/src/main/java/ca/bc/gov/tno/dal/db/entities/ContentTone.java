package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.Id;
import javax.persistence.IdClass;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * ContentTone class, provides a way to manage tone.
 */
@Entity
@IdClass(ContentTonePK.class)
@Table(name = "\"ContentTone\"")
public class ContentTone extends AuditColumns {
  /**
   * Primary key to identify the content tone.
   * Foreign key to content.
   */
  @Id
  @Column(name = "\"contentId\"", nullable = false)
  private int contentId;

  /**
   * The content reference.
   */
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "\"contentId\"", insertable = false, updatable = false)
  private Content content;

  /**
   * Primary key to identify the content tone.
   * Foreign key to tone pool.
   */
  @Id
  @Column(name = "\"tonePoolId\"", nullable = false)
  private int tonePoolId;

  /**
   * The tone pool reference.
   */
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "\"tonePoolId\"", insertable = false, updatable = false)
  private TonePool tonePool;

  /**
   * Value of tone.
   */
  @Column(name = "\"value\"", nullable = false)
  private int value;

  /**
   * Creates a new instance of a ContentTone object.
   */
  public ContentTone() {

  }

  /**
   * Creates a new instance of a ContentTone object, initializes with specified
   * parameters.
   * 
   * @param content  Content object
   * @param tonePool TonePool object
   * @param value    Tone value
   */
  public ContentTone(Content content, TonePool tonePool, int value) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (tonePool == null)
      throw new NullPointerException("Parameter 'tonePool' cannot be null.");

    this.content = content;
    this.contentId = content.getId();
    this.tonePool = tonePool;
    this.tonePoolId = tonePool.getId();
    this.value = value;
  }

  /**
   * @return int return the contentId
   */
  public int getContentId() {
    return contentId;
  }

  /**
   * @param contentId the contentId to set
   */
  public void setContentId(int contentId) {
    this.contentId = contentId;
  }

  /**
   * @return Content return the content
   */
  public Content getContent() {
    return content;
  }

  /**
   * @param content the content to set
   */
  public void setContent(Content content) {
    this.content = content;
  }

  /**
   * @return int return the tonePoolId
   */
  public int getTonePoolId() {
    return tonePoolId;
  }

  /**
   * @param tonePoolId the tonePoolId to set
   */
  public void setTonePoolId(int tonePoolId) {
    this.tonePoolId = tonePoolId;
  }

  /**
   * @return TonePool return the tonePool
   */
  public TonePool getTonePool() {
    return tonePool;
  }

  /**
   * @param tonePool the tonePool to set
   */
  public void setTonePool(TonePool tonePool) {
    this.tonePool = tonePool;
  }

  /**
   * @return int return the value
   */
  public int getValue() {
    return value;
  }

  /**
   * @param value the value to set
   */
  public void setValue(int value) {
    this.value = value;
  }

}
