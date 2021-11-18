package ca.bc.gov.tno.dal.elastic.models;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import org.springframework.data.annotation.Id;
import org.springframework.data.elasticsearch.annotations.Document;
import org.springframework.data.elasticsearch.annotations.Field;
import org.springframework.data.elasticsearch.annotations.FieldType;
import org.springframework.data.elasticsearch.annotations.Setting;

/**
 * Story class, provides a model to capture story information within
 * Elasticsearch.
 */
@Document(indexName = "story")
@Setting(settingPath = "static/es-settings.json")
public class Story {
  /**
   * Unique key to identify this story within Elasticsearch.
   */
  @Id
  private String id;

  /**
   * Kafka topic information.
   */
  private Kafka kafka;

  /**
   * Data source information.
   */
  private Source source;

  /**
   * The type of content.
   */
  @Field(type = FieldType.Keyword)
  private String type;

  /**
   * The language of the content.
   */
  @Field(type = FieldType.Keyword)
  private String language;

  /**
   * The content title.
   */
  @Field(type = FieldType.Text)
  private String title;

  /**
   * The source copyright.
   */
  @Field(type = FieldType.Text, index = false)
  private String copyright;

  /**
   * The name of the author.
   */
  @Field(type = FieldType.Text)
  private String author;

  /**
   * The content summary.
   */
  @Field(type = FieldType.Text)
  private String summary;

  /**
   * When the content was published on.
   */
  @Field(type = FieldType.Date)
  private Date publishedOn;

  /**
   * When the content was last updated on.
   */
  @Field(type = FieldType.Date)
  private Date updatedOn;

  /**
   * A collection of tags that are used to search for this content.
   */
  @Field(type = FieldType.Nested, includeInParent = true)
  private List<Tag> tags = new ArrayList<Tag>();

  /**
   * Creates a new instance of a Story object.
   */
  public Story() {
  }

  /**
   * Creates a new instance of a Story object, initializes with specified
   * parameters.
   * 
   * @param kafka  Kafka metadata information.
   * @param source Data source information.
   * @param type   Story type.
   * @param title  Title of the story.
   */
  public Story(final Kafka kafka, final Source source, final String type, final String title) {
    this.id = String.format("%s-%s-%s", kafka.getTopic(), kafka.getPartition(), kafka.getOffset());
    this.kafka = kafka;
    this.source = source;
    this.type = type;
    this.title = title;
  }

  /**
   * @return String return the id
   */
  public String getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(String id) {
    this.id = id;
  }

  /**
   * @return Kafka return the kafka
   */
  public Kafka getKafka() {
    return kafka;
  }

  /**
   * @param kafka the kafka to set
   */
  public void setKafka(Kafka kafka) {
    this.kafka = kafka;
  }

  /**
   * @return Source return the source
   */
  public Source getSource() {
    return source;
  }

  /**
   * @param source the source to set
   */
  public void setSource(Source source) {
    this.source = source;
  }

  /**
   * @return String return the type
   */
  public String getType() {
    return type;
  }

  /**
   * @param type the type to set
   */
  public void setType(String type) {
    this.type = type;
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

}
