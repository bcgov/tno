package ca.bc.gov.tno.services.models;

/**
 * ContentLink class, provides a way to manage content links.
 */
public class ContentLink extends AuditColumns {
  /**
   * Primary key to identify the content link.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content reference.
   */
  private Content content;

  /**
   * Primary key to identify the content link.
   * Foreign key to content link .
   */
  private int linkId;

  /**
   * The content link reference.
   */
  private Content link;

  /**
   * Creates a new instance of a ContentLink object.
   */
  public ContentLink() {

  }

  /**
   * Creates a new instance of a ContentLink object, initializes with
   * specified
   * parameters.
   * 
   * @param content Content object
   * @param link    Link object
   */
  public ContentLink(Content content, Content link) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (link == null)
      throw new NullPointerException("Parameter 'link' cannot be null.");

    this.content = content;
    this.contentId = content.getId();
    this.link = link;
    this.linkId = link.getId();
  }

  /**
   * Creates a new instance of a ContentLink object, initializes with
   * specified
   * parameters.
   * 
   * @param content Content object
   * @param link    Link object
   * @param version Row version value
   */
  public ContentLink(Content content, Content link, long version) {
    this(content, link);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a ContentLink object, initializes with
   * specified
   * parameters.
   * 
   * @param contentId Foreign key to Content object
   * @param linkId    Foreign key to Link object
   */
  public ContentLink(int contentId, int linkId) {
    this.contentId = contentId;
    this.linkId = linkId;
  }

  /**
   * Creates a new instance of a ContentLink object, initializes with
   * specified
   * parameters.
   * 
   * @param contentId Foreign key to Content object
   * @param linkId    Foreign key to Link object
   * @param version   Row version value
   */
  public ContentLink(int contentId, int linkId, long version) {
    this(contentId, linkId);
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
   * @return int return the linkId
   */
  public int getLinkId() {
    return linkId;
  }

  /**
   * @param linkId the linkId to set
   */
  public void setLinkId(int linkId) {
    this.linkId = linkId;
  }

  /**
   * @return Content return the link
   */
  public Content getLink() {
    return link;
  }

  /**
   * @param link the link to set
   */
  public void setLink(Content link) {
    this.link = link;
  }

}
