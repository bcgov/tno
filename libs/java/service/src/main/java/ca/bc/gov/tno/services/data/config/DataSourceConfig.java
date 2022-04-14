package ca.bc.gov.tno.services.data.config;

import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.List;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.services.models.DataSource;

/**
 * Configuration settings for Data Source.
 */
@Configuration
@ConfigurationProperties("data.source")
public class DataSourceConfig {
  /**
   * Data Source key to identify what configuration to use in the database.
   */
  private String id;

  /**
   * The data source type (i.e. Syndication, Newspaper, Radio News, ...).
   */
  private String mediaType;

  /**
   * The data source location (i.e. Internet, Streaming, SFTP, NAS, ...).
   */
  private String dataLocation;

  /**
   * Whether the data source is enabled.
   */
  private boolean enabled = true;

  /**
   * The Kafka Topic to push content into.
   */
  private String topic;

  /**
   * Date when the data source was last run.
   */
  private ZonedDateTime lastRanOn;

  /**
   * Number of times this data source has been run. This value is compared to the
   * 'repeat' configuration.
   */
  private int ranCounter;

  /**
   * Maximum number of attempts after a failure.
   */
  private int maxFailedAttempts;

  /**
   * Maximum number of failed attempts.
   */
  public int failedAttempts;

  /**
   * An array of schedules for this data source.
   */
  private List<ScheduleConfig> schedules = new ArrayList<>();

  /**
   * Creates a new instance of a DataSourceConfig object.
   */
  public DataSourceConfig() {

  }

  /**
   * Creates a new instance of a DataSourceConfig object, initializes with
   * specified parameters.
   *
   * @param dataSource The data source object.
   */
  public DataSourceConfig(DataSource dataSource) {
    if (dataSource == null)
      throw new IllegalArgumentException("Parameter 'dataSource' is required.");

    this.id = dataSource.getCode();
    this.topic = dataSource.getTopic();
    this.lastRanOn = dataSource.getLastRanOn();
    this.maxFailedAttempts = dataSource.getRetryLimit();
    this.failedAttempts = dataSource.getFailedAttempts();

    var type = dataSource.getMediaType();
    this.mediaType = type.getName();

    var location = dataSource.getDataLocation();
    this.dataLocation = location.getName();

    schedules.addAll(
        dataSource.getDataSourceSchedules().stream().map((dss) -> new ScheduleConfig(dss.getSchedule())).toList());

    var license = dataSource.getLicense();
    this.enabled = dataSource.getIsEnabled() && type.getIsEnabled() && license.getIsEnabled()
        && location.getIsEnabled();
  }

  /**
   * @return String return the id
   */
  public String getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(String id) {
    this.id = id;
  }

  /**
   * @return String return the media type
   */
  public String getMediaType() {
    return mediaType;
  }

  /**
   * @param mediaType the mediaType to set
   */
  public void setMediaType(String mediaType) {
    this.mediaType = mediaType;
  }

  /**
   * @return String return the data location
   */
  public String getDataLocation() {
    return dataLocation;
  }

  /**
   * @param dataLocation the dataLocation to set
   */
  public void setDataLocation(String dataLocation) {
    this.dataLocation = dataLocation;
  }

  /**
   * @return boolean return the enabled
   */
  public boolean getIsEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return String return the topic
   */
  public String getTopic() {
    return topic;
  }

  /**
   * @param topic the topic to set
   */
  public void setTopic(String topic) {
    this.topic = topic;
  }

  /**
   * @return ZonedDateTime return the lastRanOn
   */
  public ZonedDateTime getLastRanOn() {
    return lastRanOn;
  }

  /**
   * @param lastRanOn the lastRanOn to set
   */
  public void setLastRanOn(ZonedDateTime lastRanOn) {
    this.lastRanOn = lastRanOn;
  }

  /**
   * @return int return the ranCounter
   */
  public int getRanCounter() {
    return ranCounter;
  }

  /**
   * @param ranCounter the ranCounter to set
   */
  public void setRanCounter(int ranCounter) {
    this.ranCounter = ranCounter;
  }

  /**
   * @return int return the maxFailedAttempts
   */
  public int getMaxFailedAttempts() {
    return maxFailedAttempts;
  }

  /**
   * @param maxFailedAttempts the maxFailedAttempts to set
   */
  public void setMaxFailedAttempts(int maxFailedAttempts) {
    this.maxFailedAttempts = maxFailedAttempts;
  }

  /**
   * @return int return the failedAttempts
   */
  public int getFailedAttempts() {
    return failedAttempts;
  }

  /**
   * @param failedAttempts the failedAttempts to set
   */
  public void setFailedAttempts(int failedAttempts) {
    this.failedAttempts = failedAttempts;
  }

  /**
   * @return List{ScheduleConfig} return the schedules
   */
  public List<ScheduleConfig> getSchedules() {
    return schedules;
  }

  /**
   * @param schedules the schedules to set
   */
  public void setSchedules(List<ScheduleConfig> schedules) {
    this.schedules = schedules;
  }

}
