package ca.bc.gov.tno.areas.editor.models;

import ca.bc.gov.tno.dal.db.entities.TimeTracking;
import ca.bc.gov.tno.models.AuditColumnModel;

public class TimeTrackingModel extends AuditColumnModel {
  private int contentId;
  private int userId;
  private float effort;
  private String activity;

  public TimeTrackingModel() {
  }

  public TimeTrackingModel(TimeTracking entity) {
    super(entity);

    if (entity != null) {
      this.contentId = entity.getContentId();
      this.userId = entity.getUserId();
      this.effort = entity.getEffort();
      this.activity = entity.getActivity();
    }
  }

  /**
   * @return int the contentId
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
   * @return int the userId
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
   * @return float the effort
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
   * @return String the activity
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
