package ca.bc.gov.tno.dal.db.entities;

import java.io.Serializable;
import java.util.Objects;

/**
 * ContentLinkPK class, provides primary key for ContentLink.
 */
public class ContentLinkPK implements Serializable {
  /**
   * The content tone abbreviation.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content tone unique key.
   * Foreign key to tone pool.
   */
  private int linkId;

  /**
   * Creates a new instance of a ContentLinkPK object.
   */
  public ContentLinkPK() {

  }

  /**
   * Creates a new instance of a ContentLinkPK object, initializes with
   * specified parameters.
   * 
   * @param contentId Foreign key to content.
   * @param linkId    Foreign key to tone pool.
   */
  public ContentLinkPK(int contentId, int linkId) {
    this.contentId = contentId;
    this.linkId = linkId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.contentId);
    hash = 79 & hash + Objects.hashCode(this.linkId);
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
    final ContentLinkPK pk = (ContentLinkPK) obj;
    if (!Objects.equals(this.contentId, pk.contentId) || !Objects.equals(this.linkId, pk.linkId)) {
      return false;
    }
    return Objects.equals(this.contentId, pk.contentId) && Objects.equals(this.linkId, pk.linkId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("contentId=").append(contentId);
    sb.append(", linkId=").append(linkId);
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

}
