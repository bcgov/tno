package ca.bc.gov.tno.services.data.config;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.dal.db.entities.DataSource;

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
   * The data source type (i.e. RSS, ATOM, Newspaper, Radio News, ...).
   */
  private String mediaType;

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
  private Date lastRanOn;

  /**
   * Number of times this data source has been run. This value is compared to the
   * 'repeat' configuration.
   */
  private int ranCounter;

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

    setId(dataSource.getCode());
    setTopic(dataSource.getTopic());
    setLastRanOn(dataSource.getLastRanOn());

    var type = dataSource.getMediaType();
    setMediaType(type.getName());

    schedules.addAll(
        dataSource.getDataSourceSchedules().stream().map((dss) -> new ScheduleConfig(dss.getSchedule())).toList());

    var license = dataSource.getLicense();
    setEnabled(dataSource.isEnabled() && type.isEnabled() && license.isEnabled());
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
   * @return boolean return the enabled
   */
  public boolean isEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setEnabled(boolean enabled) {
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
   * @return Date return the lastRanOn
   */
  public Date getLastRanOn() {
    return lastRanOn;
  }

  /**
   * @param lastRanOn the lastRanOn to set
   */
  public void setLastRanOn(Date lastRanOn) {
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
