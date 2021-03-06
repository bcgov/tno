package ca.bc.gov.tno.services.models;

/**
 * PrintContent class, provides a way to store print content.
 * Print content is an optional one-to-one mapping to content.
 */
public class PrintContent extends AuditColumns {
  /**
   * Primary key to identify the content.
   */
  private int contentId;

  /**
   * The content this print belongs to.
   */
  private Content content;

  /**
   * The edition the content was from.
   */
  private String edition = "";

  /**
   * The section the content was from.
   */
  private String section = "";

  /**
   * The storyType of the content.
   */
  private String storyType = "";

  /**
   * The byline of the content.
   */
  private String byline = "";

  /**
   * Creates a new instance of a PrintContent object.
   */
  public PrintContent() {
  }

  /**
   * Creates a new instance of a PrintContent object, initializes with
   * specified parameters.
   * 
   * @param content   The content this print content belongs with
   * @param edition   The edition of the content
   * @param section   The section of the content
   * @param storyType The story type of the content
   * @param byline    The byline of the content
   */
  public PrintContent(Content content, String edition, String section, String storyType, String byline) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (edition == null)
      throw new NullPointerException("Parameter 'edition' cannot be null.");
    if (section == null)
      throw new NullPointerException("Parameter 'section' cannot be null.");
    if (section.length() == 0)
      throw new IllegalArgumentException("Parameter 'section' cannot be empty.");
    if (storyType == null)
      throw new NullPointerException("Parameter 'storyType' cannot be null.");
    if (storyType.length() == 0)
      throw new IllegalArgumentException("Parameter 'storyType' cannot be empty.");
    if (byline == null)
      throw new NullPointerException("Parameter 'byline' cannot be null.");
    if (byline.length() == 0)
      throw new IllegalArgumentException("Parameter 'byline' cannot be empty.");

    this.content = content;
    this.contentId = content.getId();
    this.edition = edition;
    this.section = section;
    this.storyType = storyType;
    this.byline = byline;
  }

  /**
   * Creates a new instance of a PrintContent object, initializes with
   * specified parameters.
   * 
   * @param content   The content this print content belongs with
   * @param edition   The edition of the content
   * @param section   The section of the content
   * @param storyType The story type of the content
   * @param byline    The byline of the content
   * @param version   Row version value
   */
  public PrintContent(Content content, String edition, String section, String storyType,
      String byline, long version) {
    this(content, edition, section, storyType, byline);
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
   * @return String return the section
   */
  public String getSection() {
    return section;
  }

  /**
   * @param section the section to set
   */
  public void setSection(String section) {
    this.section = section;
  }

  /**
   * @return String return the edition
   */
  public String getEdition() {
    return edition;
  }

  /**
   * @param edition the edition to set
   */
  public void setEdition(String edition) {
    this.edition = edition;
  }

  /**
   * @return String return the storyType
   */
  public String getStoryType() {
    return storyType;
  }

  /**
   * @param storyType the storyType to set
   */
  public void setStoryType(String storyType) {
    this.storyType = storyType;
  }

  /**
   * @return String return the byline
   */
  public String getByline() {
    return byline;
  }

  /**
   * @param byline the byline to set
   */
  public void setByline(String byline) {
    this.byline = byline;
  }

}
