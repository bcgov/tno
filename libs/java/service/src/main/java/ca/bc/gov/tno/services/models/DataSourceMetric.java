package ca.bc.gov.tno.services.models;

/**
 * DataSourceMetric class, provides a way to manage dataSource metrics.
 */
public class DataSourceMetric extends AuditColumns {
  /**
   * Primary key to identify the dataSource metric.
   * Foreign key to dataSource.
   */
  private int dataSourceId;

  /**
   * The dataSource reference.
   */
  private DataSource dataSource;

  /**
   * Primary key to identify the dataSource metric.
   * Foreign key to metric .
   */
  private int sourceMetricId;

  /**
   * The metric reference.
   */
  private SourceMetric sourceMetric;

  /**
   * Value of metric.
   */
  private double reach;

  /**
   * Value of metric.
   */
  private double earned;

  /**
   * Value of metric.
   */
  private double impression;

  /**
   * Creates a new instance of a DataSourceMetric object.
   */
  public DataSourceMetric() {

  }

  /**
   * Creates a new instance of a DataSourceMetric object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSource DataSource object
   * @param metric     SourceMetric object
   * @param reach      SourceMetric reach value
   * @param earned     SourceMetric earned value
   * @param impression SourceMetric impression value
   */
  public DataSourceMetric(DataSource dataSource, SourceMetric metric, double reach, double earned, double impression) {
    if (dataSource == null)
      throw new NullPointerException("Parameter 'dataSource' cannot be null.");
    if (metric == null)
      throw new NullPointerException("Parameter 'metric' cannot be null.");

    this.dataSource = dataSource;
    this.dataSourceId = dataSource.getId();
    this.sourceMetric = metric;
    this.sourceMetricId = metric.getId();
    this.reach = reach;
    this.earned = earned;
    this.impression = impression;
  }

  /**
   * Creates a new instance of a DataSourceMetric object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSource DataSource object
   * @param metric     SourceMetric object
   * @param reach      SourceMetric reach value
   * @param earned     SourceMetric earned value
   * @param impression SourceMetric impression value
   * @param version    Row version value
   */
  public DataSourceMetric(DataSource dataSource, SourceMetric metric, double reach, double earned, double impression,
      long version) {
    this(dataSource, metric, reach, earned, impression);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a DataSourceMetric object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSource     DataSource object
   * @param sourceMetricId Foreign key to Metric object
   * @param reach          SourceMetric reach value
   * @param earned         SourceMetric earned value
   * @param impression     SourceMetric impression value
   */
  public DataSourceMetric(DataSource dataSource, int sourceMetricId, double reach, double earned, double impression) {
    if (dataSource == null)
      throw new NullPointerException("Parameter 'dataSource' cannot be null.");

    this.dataSource = dataSource;
    this.dataSourceId = dataSource.getId();
    this.sourceMetricId = sourceMetricId;
    this.reach = reach;
    this.earned = earned;
    this.impression = impression;
  }

  /**
   * Creates a new instance of a DataSourceMetric object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSource     DataSource object
   * @param sourceMetricId Foreign key to Metric object
   * @param reach          SourceMetric reach value
   * @param earned         SourceMetric earned value
   * @param impression     SourceMetric impression value
   * @param version        Row version value
   */
  public DataSourceMetric(DataSource dataSource, int sourceMetricId, double reach, double earned, double impression,
      long version) {
    this(dataSource, sourceMetricId, reach, earned, impression);
    this.setVersion(version);
  }

  /**
   * Creates a new instance of a DataSourceMetric object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSourceId   Foreign key to DataSource object
   * @param sourceMetricId Foreign key to Metric object
   * @param reach          SourceMetric reach value
   * @param earned         SourceMetric earned value
   * @param impression     SourceMetric impression value
   */
  public DataSourceMetric(int dataSourceId, int sourceMetricId, double reach, double earned, double impression) {
    this.dataSourceId = dataSourceId;
    this.sourceMetricId = sourceMetricId;
    this.reach = reach;
    this.earned = earned;
    this.impression = impression;
  }

  /**
   * Creates a new instance of a DataSourceMetric object, initializes with
   * specified
   * parameters.
   * 
   * @param dataSourceId   Foreign key to DataSource object
   * @param sourceMetricId Foreign key to Metric object
   * @param reach          SourceMetric reach value
   * @param earned         SourceMetric earned value
   * @param impression     SourceMetric impression value
   * @param version        Row version value
   */
  public DataSourceMetric(int dataSourceId, int sourceMetricId, double reach, double earned, double impression,
      long version) {
    this(dataSourceId, sourceMetricId, reach, earned, impression);
    this.setVersion(version);
  }

  /**
   * @return int return the dataSourceId
   */
  public int getDataSourceId() {
    return dataSourceId;
  }

  /**
   * @param dataSourceId the dataSourceId to set
   */
  public void setDataSourceId(int dataSourceId) {
    this.dataSourceId = dataSourceId;
  }

  /**
   * @return DataSource return the dataSource
   */
  public DataSource getDataSource() {
    return dataSource;
  }

  /**
   * @param dataSource the dataSource to set
   */
  public void setDataSource(DataSource dataSource) {
    this.dataSource = dataSource;
  }

  /**
   * @return int return the sourceMetricId
   */
  public int getSourceMetricId() {
    return sourceMetricId;
  }

  /**
   * @param sourceMetricId the sourceMetricId to set
   */
  public void setSourceMetricId(int sourceMetricId) {
    this.sourceMetricId = sourceMetricId;
  }

  /**
   * @return Metric return the sourceMetric
   */
  public SourceMetric getSourceMetric() {
    return sourceMetric;
  }

  /**
   * @param sourceMetric the sourceMetric to set
   */
  public void setSourceMetric(SourceMetric sourceMetric) {
    this.sourceMetric = sourceMetric;
  }

  /**
   * @return double return the reach
   */
  public double getReach() {
    return reach;
  }

  /**
   * @param reach the reach to set
   */
  public void setReach(double reach) {
    this.reach = reach;
  }

  /**
   * @return double return the earned
   */
  public double getEarned() {
    return earned;
  }

  /**
   * @param earned the earned to set
   */
  public void setEarned(double earned) {
    this.earned = earned;
  }

  /**
   * @return double return the impression
   */
  public double getImpression() {
    return impression;
  }

  /**
   * @param impression the impression to set
   */
  public void setImpression(double impression) {
    this.impression = impression;
  }

}
