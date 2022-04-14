package ca.bc.gov.tno.services.models;

import java.io.Serializable;
import java.util.Objects;

/**
 * ContentActionPK class, provides primary key for ContentAction.
 */
public class ContentActionPK implements Serializable {
  /**
   * The content action abbreviation.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content action unique key.
   * Foreign key to action.
   */
  private int actionId;

  /**
   * Creates a new instance of a ContentActionPK object.
   */
  public ContentActionPK() {

  }

  /**
   * Creates a new instance of a ContentActionPK object, initializes with
   * specified parameters.
   * 
   * @param contentId Foreign key to content.
   * @param actionId  Foreign key to action.
   */
  public ContentActionPK(int contentId, int actionId) {
    this.contentId = contentId;
    this.actionId = actionId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.contentId);
    hash = 79 & hash + Objects.hashCode(this.actionId);
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
    final ContentActionPK pk = (ContentActionPK) obj;
    if (!Objects.equals(this.contentId, pk.contentId) || !Objects.equals(this.actionId, pk.actionId)) {
      return false;
    }
    return Objects.equals(this.contentId, pk.contentId) && Objects.equals(this.actionId, pk.actionId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("contentId=").append(contentId);
    sb.append(", actionId=").append(actionId);
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
   * @return int return the actionId
   */
  public int getActionId() {
    return actionId;
  }

  /**
   * @param actionId the actionId to set
   */
  public void setActionId(int actionId) {
    this.actionId = actionId;
  }

}
