package ca.bc.gov.tno.dal.elastic.models;

import org.springframework.data.elasticsearch.annotations.Field;
import org.springframework.data.elasticsearch.annotations.FieldType;

/**
 * Source class, provides a model to represent the data source this story
 * originated from.
 */
public class Source {
  /**
   * The source of the content, which can be a URL or identifier of the source
   * like their name.
   */
  @Field(type = FieldType.Keyword)
  private String name;

  /**
   * The unique identifier from the source that identifies this content
   * throughout.
   */
  @Field(type = FieldType.Keyword, index = false)
  private String uid;

  /**
   * URL to the original content.
   */
  @Field(type = FieldType.Keyword, index = false)
  private String link;

  /**
   * The path to the source content file (audio/video/image).
   */
  @Field(type = FieldType.Keyword, index = false)
  private String filePath;

  /**
   * The URL to the stream.
   */
  @Field(type = FieldType.Keyword, index = false)
  private String streamUrl;

  /**
   * Creates a new instance of a Source object.
   */
  public Source() {

  }

  /**
   * Creates a new instance of a Source object, initializes with specified
   * parameters.
   * 
   * @param name The data source name.
   * @param uid  The unique identity from the original data source for this story.
   */
  public Source(final String name, final String uid) {
    this.name = name;
    this.uid = uid;
  }

  /**
   * @return String return the name
   */
  public String getName() {
    return name;
  }

  /**
   * @param name the name to set
   */
  public void setName(String name) {
    this.name = name;
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

}
