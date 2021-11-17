package ca.bc.gov.tno.models;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import ca.bc.gov.tno.ContentType;

/**
 * ContentBase class, provides a model for capturing all the relevant data that
 * will make content searchable.
 */
public abstract class ContentBase {
  /**
   * The source of the content, which can be a URL or identifier of the source
   * like their name.
   */
  private String source;

  /**
   * The type of content.
   */
  private ContentType type;

  /**
   * The unique identifier from the source that identifies this content
   * throughout.
   */
  private String uid;

  /**
   * URL to the original content.
   */
  private String link;

  /**
   * The language of the content.
   */
  private String language;

  /**
   * The source copyright.
   */
  private String copyright;

  /**
   * The name of the author.
   */
  private String author;

  /**
   * The content title.
   */
  private String title;

  /**
   * The content summary.
   */
  private String summary;

  /**
   * The path to the source content file (audio/video/image).
   */
  private String filePath;

  /**
   * The URL to the stream.
   */
  private String streamUrl;

  /**
   * When the content was published on.
   */
  private Date publishedOn;

  /**
   * When the content was last updated on.
   */
  private Date updatedOn;

  /**
   * A collection of tags that are used to search for this content.
   */
  private List<Tag> tags = new ArrayList<Tag>();

  /**
   * Creates a new instance of a ContentBase object.
   */
  public ContentBase() {
  }

  /**
   * Creates a new instance of a ContentBase object, initializes with specified
   * parameters.
   * 
   * @param source The unique identifier of the source (i.e. code).
   * @param type   The type of content [text|audio|video].
   * @param uid    A unique identifier for the content.
   * @param title  The content title.
   */
  public ContentBase(String source, ContentType type, String uid, String title) {
    if (source == null || source.length() == 0)
      throw new IllegalArgumentException("Parameter 'source' is required, and cannot be empty.");
    if (uid == null || uid.length() == 0)
      throw new IllegalArgumentException("Parameter 'uid' is required, and cannot be empty.");
    if (title == null || title.length() == 0)
      throw new IllegalArgumentException("Parameter 'title' is required, and cannot be empty.");

    this.source = source;
    this.type = type;
    this.uid = uid;
    this.title = title;
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
   * @return ContentType return the type
   */
  public ContentType getType() {
    return type;
  }

  /**
   * @param type the type to set
   */
  public void setType(ContentType type) {
    this.type = type;
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
   * @return String return the title
   */
  public String getTitle() {
    return title;
  }

  /**
   * @param title the title to set
   */
  public void setTitle(String title) {
    this.title = title;
  }

  /**
   * @return String return the summary
   */
  public String getSummary() {
    return summary;
  }

  /**
   * @param summary the summary to set
   */
  public void setSummary(String summary) {
    this.summary = summary;
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
   * @return Date return the updatedOn
   */
  public Date getUpdatedOn() {
    return updatedOn;
  }

  /**
   * @param updatedOn the updatedOn to set
   */
  public void setUpdatedOn(Date updatedOn) {
    this.updatedOn = updatedOn;
  }

  /**
   * @return List{Tag} return the tags
   */
  public List<Tag> getTags() {
    return tags;
  }

  /**
   * @param tags the tags to set
   */
  public void setTags(List<Tag> tags) {
    this.tags = tags;
  }

  /**
   * @return String return the link
   */
  public String getLink() {
    return link;
  }

  /**
   * @param link the link to set
   */
  public void setLink(String link) {
    this.link = link;
  }

  /**
   * @return String return the language
   */
  public String getLanguage() {
    return language;
  }

  /**
   * @param language the language to set
   */
  public void setLanguage(String language) {
    this.language = language;
  }

  /**
   * @return String return the author
   */
  public String getAuthor() {
    return author;
  }

  /**
   * @param author the author to set
   */
  public void setAuthor(String author) {
    this.author = author;
  }

  /**
   * @return String return the filePath
   */
  public String getFilePath() {
    return filePath;
  }

  /**
   * @param filePath the filePath to set
   */
  public void setFilePath(String filePath) {
    this.filePath = filePath;
  }

  /**
   * @return String return the streamUrl
   */
  public String getStreamUrl() {
    return streamUrl;
  }

  /**
   * @param streamUrl the streamUrl to set
   */
  public void setStreamUrl(String streamUrl) {
    this.streamUrl = streamUrl;
  }

  /**
   * @return String return the copyright
   */
  public String getCopyright() {
    return copyright;
  }

  /**
   * @param copyright the copyright to set
   */
  public void setCopyright(String copyright) {
    this.copyright = copyright;
  }

}
