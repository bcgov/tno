package ca.bc.gov.tno.services.models;

import java.io.Serializable;
import java.util.Objects;

/**
 * ContentTypeActionPK class, provides primary key for ContentTypeAction.
 */
public class ContentTypeActionPK implements Serializable {
  /**
   * The content type action primary key.
   * Foreign key to content type.
   */
  private int contentTypeId;

  /**
   * The content type action primary key.
   * Foreign key to action.
   */
  private int actionId;

  /**
   * Creates a new instance of a ContentTypeActionPK object.
   */
  public ContentTypeActionPK() {

  }

  /**
   * Creates a new instance of a ContentTypeActionPK object, initializes with
   * specified parameters.
   * 
   * @param contentTypeId Foreign key to content type.
   * @param actionId      Foreign key to action.
   */
  public ContentTypeActionPK(int contentTypeId, int actionId) {
    this.contentTypeId = contentTypeId;
    this.actionId = actionId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.contentTypeId);
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
    final ContentTypeActionPK pk = (ContentTypeActionPK) obj;
    if (!Objects.equals(this.contentTypeId, pk.contentTypeId) || !Objects.equals(this.actionId, pk.actionId)) {
      return false;
    }
    return Objects.equals(this.contentTypeId, pk.contentTypeId) && Objects.equals(this.actionId, pk.actionId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("contentTypeId=").append(contentTypeId);
    sb.append(", actionId=").append(actionId);
    sb.append("}");
    return sb.toString();
  }

  /**
   * @return int return the contentTypeId
   */
  public int getContentTypeId() {
    return contentTypeId;
  }

  /**
   * @param contentTypeId the contentTypeId to set
   */
  public void setContentTypeId(int contentTypeId) {
    this.contentTypeId = contentTypeId;
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
