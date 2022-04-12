package ca.bc.gov.tno.areas.admin.models;

import javax.persistence.Persistence;

import ca.bc.gov.tno.dal.db.entities.DataSourceMetric;

public class DataSourceMetricModel extends SourceMetricModel {

  /**
   * The value given to the metric.
   */
  private double reach;

  /**
   * The value given to the metric.
   */
  private double earned;

  /**
   * The value given to the metric.
   */
  private double impression;

  public DataSourceMetricModel() {
  }

  public DataSourceMetricModel(DataSourceMetric entity) {
    if (entity != null) {
      var putil = Persistence.getPersistenceUtil();

      if (putil.isLoaded(entity, "sourceMetric")) {
        var metric = entity.getSourceMetric();
        this.setId(metric.getId());
        this.setName(metric.getName());
        this.setDescription(metric.getDescription());
        this.setIsEnabled(metric.getIsEnabled());
        this.setSortOrder(metric.getSortOrder());
      }

      this.reach = entity.getReach();
      this.earned = entity.getEarned();
      this.impression = entity.getImpression();
      this.setVersion(entity.getVersion());
    }
  }

  /**
   * @return double the reach
   */
  public double getReach() {
    return reach;
  }

  /**
   * @param sortOrder the reach to set
   */
  public void setReach(double reach) {
    this.reach = reach;
  }

  /**
   * @return double the earned
   */
  public double getEarned() {
    return earned;
  }

  /**
   * @param sortOrder the earned to set
   */
  public void setEarned(double earned) {
    this.earned = earned;
  }

  /**
   * @return double the impression
   */
  public double getImpression() {
    return impression;
  }

  /**
   * @param sortOrder the impression to set
   */
  public void setImpression(double impression) {
    this.impression = impression;
  }

}
