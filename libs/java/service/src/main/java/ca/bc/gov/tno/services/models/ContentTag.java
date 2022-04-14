package ca.bc.gov.tno.services.models;

/**
 * ContentTag class, provides a way to manage content tags.
 */
public class ContentTag extends AuditColumns {
  /**
   * Primary key to identify the content tag.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content reference.
   */
  private Content content;

  /**
   * Primary key to identify the content tag.
   * Foreign key to tag .
   */
  private String tagId;

  /**
   * The tag reference.
   */
  private Tag tag;

  /**
   * Creates a new instance of a ContentTag object.
   */
  public ContentTag() {

  }

  /**
   * Creates a new instance of a ContentTag object, initializes with
   * specified
   * parameters.
   * 
   * @param content Content object
   * @param tag     Tag object
   */
  public ContentTag(Content content, Tag tag) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (tag == null)
      throw new NullPointerException("Parameter 'tag' cannot be null.");

    this.content = content;
    this.contentId = content.getId();
    this.tag = tag;
    this.tagId = tag.getId();
  }

  /**
   * Creates a new instance of a ContentTag object, initializes with
   * specified
   * parameters.
   * 
   * @param content Content object
   * @param tag     Tag object
   * @param version Row version value
   */
  public ContentTag(Content content, Tag tag, long version) {
    this(content, tag);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a ContentTag object, initializes with
   * specified
   * parameters.
   * 
   * @param content Content object
   * @param tagId   Foreign key to Tag object
   */
  public ContentTag(Content content, String tagId) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (tagId == null)
      throw new NullPointerException("Parameter 'tagId' cannot be null.");
    if (tagId.length() == 0)
      throw new IllegalArgumentException("Parameter 'tagId' cannot be empty.");

    this.content = content;
    this.contentId = content.getId();
    this.tagId = tagId;
  }

  /**
   * Creates a new instance of a ContentTag object, initializes with
   * specified
   * parameters.
   * 
   * @param content Content object
   * @param tagId   Foreign key to Tag object
   * @param version Row version value
   */
  public ContentTag(Content content, String tagId, long version) {
    this(content, tagId);
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
   * @return String return the tagId
   */
  public String getTagId() {
    return tagId;
  }

  /**
   * @param tagId the tagId to set
   */
  public void setTagId(String tagId) {
    this.tagId = tagId;
  }

  /**
   * @return Tag return the tag
   */
  public Tag getTag() {
    return tag;
  }

  /**
   * @param tag the tag to set
   */
  public void setTag(Tag tag) {
    this.tag = tag;
  }
}
