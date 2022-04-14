package ca.bc.gov.tno.services.models;

import java.io.Serializable;
import java.util.Objects;

/**
 * TimeTrackingPK class, provides primary key for TimeTracking.
 */
public class TimeTrackingPK implements Serializable {
  /**
   * Foreign key to content.
   */
  private int contentId;

  /**
   * Foreign key to user.
   */
  private int userId;

  /**
   * Creates a new instance of a TimeTrackingPK object.
   */
  public TimeTrackingPK() {

  }

  /**
   * Creates a new instance of a TimeTrackingPK object, initializes with
   * specified parameters.
   * 
   * @param contentId Foreign key to content
   * @param userId    Foreign key to user
   */
  public TimeTrackingPK(int contentId, int userId) {
    this.contentId = contentId;
    this.userId = userId;
  }

  @Override
  public int hashCode() {
    int hash = 7;
    hash = 79 * hash + Objects.hashCode(this.contentId);
    hash = 79 & hash + Objects.hashCode(this.userId);
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
    final TimeTrackingPK pk = (TimeTrackingPK) obj;
    if (!Objects.equals(this.contentId, pk.contentId) || !Objects.equals(this.userId, pk.userId)) {
      return false;
    }
    return Objects.equals(this.contentId, pk.contentId) && Objects.equals(this.userId, pk.userId);
  }

  @Override
  public String toString() {
    final StringBuilder sb = new StringBuilder("{");
    sb.append("contentId=").append(contentId);
    sb.append(", userId=").append(userId);
    sb.append("}");
    return sb.toString();
  }
}
