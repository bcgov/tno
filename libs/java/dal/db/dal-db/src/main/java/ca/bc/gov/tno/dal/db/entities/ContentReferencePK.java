package ca.bc.gov.tno.dal.db.entities;

import java.io.Serializable;
import java.util.Objects;

/**
 * ContentReferencePK class, provides primary key for ContentReference.
 */
public class ContentReferencePK implements Serializable {
  /**
   * The data source abbreviation.
   */
  private String source;

  /**
   * The source content unique key.
   */
  private String uid;

  /**
   * Creates a new instance of a ContentReferencePK object.
   */
  public ContentReferencePK() {

  }

  /**
   * Creates a new instance of a ContentReferencePK object, initializes with
   * specified parameters.
   * 
   * @param source Data source abbreviation
   * @param uid    Unique identify of the content
   */
  public ContentReferencePK(String source, String uid) {
    this.source = source;
    this.uid = uid;
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

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.source);
    hash = 79 & hash + Objects.hashCode(this.uid);
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
    final ContentReferencePK pk = (ContentReferencePK) obj;
    if (!Objects.equals(this.source, pk.source) || !Objects.equals(this.uid, pk.uid)) {
      return false;
    }
    return Objects.equals(this.source, pk.source) && Objects.equals(this.uid, pk.uid);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("source=").append(source);
    sb.append(", uid='").append(uid).append("\'");
    sb.append("}");
    return sb.toString();
  }
}
