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
 * TimeTracking class, provides a way to manage time tracking.
 */
@Entity
@IdClass(TimeTrackingPK.class)
@Table(name = "time_tracking", schema = "public")
public class TimeTracking extends AuditColumns {
  /**
   * Primary key to identify the time tracking.
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
   * Primary key to identify the time tracking.
   * Foreign key to user.
   */
  @Id
  @Column(name = "user_id", nullable = false)
  private int userId;

  /**
   * The tone pool reference.
   */
  @JsonBackReference("timeTracking")
  @ManyToOne(fetch = FetchType.LAZY)
  @JoinColumn(name = "user_id", insertable = false, updatable = false)
  private User user;

  /**
   * Effort time in minutes spent on content.
   */
  @Column(name = "effort", nullable = false)
  private float effort;

  /**
   * Description of activity.
   */
  @Column(name = "activity", nullable = false)
  private String activity;

  /**
   * Creates a new instance of a TimeTracking object.
   */
  public TimeTracking() {

  }

  /**
   * Creates a new instance of a TimeTracking object, initializes with specified
   * parameters.
   * 
   * @param content  Content object
   * @param user     User object
   * @param effort   Number of minutes
   * @param activity Description of effort
   */
  public TimeTracking(Content content, User user, float effort, String activity) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (user == null)
      throw new NullPointerException("Parameter 'user' cannot be null.");
    if (effort < 0)
      throw new IllegalArgumentException("Parameter 'effort' cannot be less than zero.");
    if (activity == null)
      throw new NullPointerException("Parameter 'activity' cannot be null.");
    if (activity.length() == 0)
      throw new IllegalArgumentException("Parameter 'activity' cannot be empty.");

    this.content = content;
    this.contentId = content.getId();
    this.user = user;
    this.userId = user.getId();
    this.effort = effort;
    this.activity = activity;
  }

  /**
   * Creates a new instance of a TimeTracking object, initializes with specified
   * parameters.
   * 
   * @param content  Content object
   * @param userId   Foreign key to User object
   * @param effort   Number of minutes
   * @param activity Description of effort
   */
  public TimeTracking(Content content, int userId, float effort, String activity) {
    if (content == null)
      throw new NullPointerException("Parameter 'content' cannot be null.");
    if (effort < 0)
      throw new IllegalArgumentException("Parameter 'effort' cannot be less than zero.");
    if (activity == null)
      throw new NullPointerException("Parameter 'activity' cannot be null.");
    if (activity.length() == 0)
      throw new IllegalArgumentException("Parameter 'activity' cannot be empty.");

    this.content = content;
    this.contentId = content.getId();
    this.userId = userId;
    this.effort = effort;
    this.activity = activity;
  }

  /**
   * Creates a new instance of a TimeTracking object, initializes with specified
   * parameters.
   * 
   * @param contentId Foreign key to Content object
   * @param userId    Foreign key to User object
   * @param effort    Number of minutes
   * @param activity  Description of effort
   */
  public TimeTracking(int contentId, int userId, float effort, String activity) {
    if (effort < 0)
      throw new IllegalArgumentException("Parameter 'effort' cannot be less than zero.");
    if (activity == null)
      throw new NullPointerException("Parameter 'activity' cannot be null.");
    if (activity.length() == 0)
      throw new IllegalArgumentException("Parameter 'activity' cannot be empty.");

    this.contentId = contentId;
    this.userId = userId;
    this.effort = effort;
    this.activity = activity;
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
   * @return int return the userId
   */
  public int getUserId() {
    return userId;
  }

  /**
   * @param userId the userId to set
   */
  public void setUserId(int userId) {
    this.userId = userId;
  }

  /**
   * @return User return the user
   */
  public User getUser() {
    return user;
  }

  /**
   * @param user the user to set
   */
  public void setUser(User user) {
    this.user = user;
  }

  /**
   * @return float return the effort
   */
  public float getEffort() {
    return effort;
  }

  /**
   * @param effort the effort to set
   */
  public void setEffort(float effort) {
    this.effort = effort;
  }

  /**
   * @return String return the activity
   */
  public String getActivity() {
    return activity;
  }

  /**
   * @param activity the activity to set
   */
  public void setActivity(String activity) {
    this.activity = activity;
  }

}
