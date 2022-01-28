package ca.bc.gov.tno.dal.db.entities;

import java.io.Serializable;
import java.util.Objects;

/**
 * ContentTonePK class, provides primary key for ContentTone.
 */
public class ContentTonePK implements Serializable {
  /**
   * The content tone abbreviation.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content tone unique key.
   * Foreign key to tone pool.
   */
  private int tonePoolId;

  /**
   * Creates a new instance of a ContentTonePK object.
   */
  public ContentTonePK() {

  }

  /**
   * Creates a new instance of a ContentTonePK object, initializes with
   * specified parameters.
   * 
   * @param contentId  Foreign key to content.
   * @param tonePoolId Foreign key to tone pool.
   */
  public ContentTonePK(int contentId, int tonePoolId) {
    this.contentId = contentId;
    this.tonePoolId = tonePoolId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.contentId);
    hash = 79 & hash + Objects.hashCode(this.tonePoolId);
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
    final ContentTonePK pk = (ContentTonePK) obj;
    if (!Objects.equals(this.contentId, pk.contentId) || !Objects.equals(this.tonePoolId, pk.tonePoolId)) {
      return false;
    }
    return Objects.equals(this.contentId, pk.contentId) && Objects.equals(this.tonePoolId, pk.tonePoolId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("contentId=").append(contentId);
    sb.append(", tonePoolId=").append(tonePoolId);
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
   * @return int return the tonePoolId
   */
  public int getTonePoolId() {
    return tonePoolId;
  }

  /**
   * @param tonePoolId the tonePoolId to set
   */
  public void setTonePoolId(int tonePoolId) {
    this.tonePoolId = tonePoolId;
  }

}
