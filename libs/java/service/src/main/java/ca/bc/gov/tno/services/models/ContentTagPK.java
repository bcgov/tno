package ca.bc.gov.tno.services.models;

import java.io.Serializable;
import java.util.Objects;

/**
 * ContentTagPK class, provides primary key for ContentTag.
 */
public class ContentTagPK implements Serializable {
  /**
   * The content tag abbreviation.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content tag unique key.
   * Foreign key to tag.
   */
  private String tagId;

  /**
   * Creates a new instance of a ContentTagPK object.
   */
  public ContentTagPK() {

  }

  /**
   * Creates a new instance of a ContentTagPK object, initializes with
   * specified parameters.
   * 
   * @param contentId Foreign key to content.
   * @param tagId     Foreign key to tag.
   */
  public ContentTagPK(int contentId, String tagId) {
    this.contentId = contentId;
    this.tagId = tagId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.contentId);
    hash = 79 & hash + Objects.hashCode(this.tagId);
    return hash;
  }

  @Override
  public boolean equals(Object obj) {
    if (this == obj) {
      return true;
    }
    if (obj == null) {
      return false;
    }
    if (getClass() != obj.getClass()) {
      return false;
    }
    final ContentTagPK pk = (ContentTagPK) obj;
    if (!Objects.equals(this.contentId, pk.contentId) || !Objects.equals(this.tagId, pk.tagId)) {
      return false;
    }
    return Objects.equals(this.contentId, pk.contentId) && Objects.equals(this.tagId, pk.tagId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("contentId=").append(contentId);
    sb.append(", tagId='").append(tagId).append("'");
    sb.append("}");
    return sb.toString();
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

}
