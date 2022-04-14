package ca.bc.gov.tno.services.models;

/**
 * ContentTone class, provides a way to manage tone.
 */
public class ContentTone extends AuditColumns {
  /**
   * Primary key to identify the content tone.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content reference.
   */
  private Content content;

  /**
   * Primary key to identify the content tone.
   * Foreign key to tone pool.
   */
  private int tonePoolId;

  /**
   * The tone pool reference.
   */
  private TonePool tonePool;

  /**
   * Value of tone.
   */
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
   * Creates a new instance of a ContentTone object, initializes with specified
   * parameters.
   * 
   * @param content  Content object
   * @param tonePool TonePool object
   * @param value    Tone value
   * @param version  Row version value
   */
  public ContentTone(Content content, TonePool tonePool, int value, long version) {
    this(content, tonePool, value);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a ContentTone object, initializes with specified
   * parameters.
   * 
   * @param content    Content object
   * @param tonePoolId Foreign key to TonePool object
   * @param value      Tone value
   */
  public ContentTone(Content content, int tonePoolId, int value) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");

    this.content = content;
    this.contentId = content.getId();
    this.tonePoolId = tonePoolId;
    this.value = value;
  }

  /**
   * Creates a new instance of a ContentTone object, initializes with specified
   * parameters.
   * 
   * @param content    Content object
   * @param tonePoolId Foreign key to TonePool object
   * @param value      Tone value
   * @param version    Row version value
   */
  public ContentTone(Content content, int tonePoolId, int value, long version) {
    this(content, tonePoolId, value);
    this.setVersion(version);
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
