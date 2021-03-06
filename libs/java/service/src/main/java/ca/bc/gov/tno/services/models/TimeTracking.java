package ca.bc.gov.tno.services.models;

/**
 * TimeTracking class, provides a way to manage time tracking.
 */
public class TimeTracking extends AuditColumns {
  /**
   * Primary key to identify the time tracking.
   * Foreign key to content.
   */
  private int contentId;

  /**
   * The content reference.
   */
  private Content content;

  /**
   * Primary key to identify the time tracking.
   * Foreign key to user.
   */
  private int userId;

  /**
   * The tone pool reference.
   */
  private User user;

  /**
   * Effort time in minutes spent on content.
   */
  private float effort;

  /**
   * Description of activity.
   */
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
   * @param user     User object
   * @param effort   Number of minutes
   * @param activity Description of effort
   * @param version  Row version value
   */
  public TimeTracking(Content content, User user, float effort, String activity, long version) {
    this(content, user, effort, activity);
    this.setVersion(version);
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
   * @param content  Content object
   * @param userId   Foreign key to User object
   * @param effort   Number of minutes
   * @param activity Description of effort
   * @param version  Row version value
   */
  public TimeTracking(Content content, int userId, float effort, String activity, long version) {
    this(content, userId, effort, activity);
    this.setVersion(version);
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
   * Creates a new instance of a TimeTracking object, initializes with specified
   * parameters.
   * 
   * @param contentId Foreign key to Content object
   * @param userId    Foreign key to User object
   * @param effort    Number of minutes
   * @param activity  Description of effort
   * @param version   Row version value
   */
  public TimeTracking(int contentId, int userId, float effort, String activity, long version) {
    this(contentId, userId, effort, activity);
    this.setVersion(version);
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
