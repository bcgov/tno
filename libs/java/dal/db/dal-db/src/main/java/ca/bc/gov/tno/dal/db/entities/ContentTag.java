package ca.bc.gov.tno.dal.db.entities;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.Id;
import javax.persistence.IdClass;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;
import com.fasterxml.jackson.annotation.JsonIgnore;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * ContentTag class, provides a way to manage content tags.
 */
@Entity
@IdClass(ContentTagPK.class)
@Table(name = "content_tag", schema = "public")
public class ContentTag extends AuditColumns {
  /**
   * Primary key to identify the content tag.
   * Foreign key to content.
   */
  @Id
  @Column(name = "content_id", nullable = false)
  private int contentId;

  /**
   * The content reference.
   */
  @JsonIgnore
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "content_id", insertable = false, updatable = false)
  private Content content;

  /**
   * Primary key to identify the content tag.
   * Foreign key to tag .
   */
  @Id
  @Column(name = "tag_id", nullable = false)
  private String tagId;

  /**
   * The tag reference.
   */
  @JsonBackReference("tag")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "tag_id", insertable = false, updatable = false)
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
