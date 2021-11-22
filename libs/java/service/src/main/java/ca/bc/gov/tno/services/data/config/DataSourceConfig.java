package ca.bc.gov.tno.services.data.config;

import java.util.Date;
import java.util.EnumSet;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.dal.db.Months;
import ca.bc.gov.tno.dal.db.WeekDays;
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
  private String type;

  /**
   * Millisecond delay between each request.
   */
  private int delay;

  /**
   * Whether the data source is enabled.
   */
  private boolean enabled = true;

  /**
   * The Kafka Topic to push content into.
   */
  private String topic;

  /**
   * Date and time to run the service on.
   */
  private Date runAt;

  /**
   * Number of time to run.
   */
  private int repeat;

  /**
   * Days of week to run on.
   */
  private EnumSet<WeekDays> runOnWeekDays;

  /**
   * Months to run on.
   */
  private EnumSet<Months> runOnMonths;

  /**
   * Day of month to run on.
   */
  private int dayOfMonth;

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

    var type = dataSource.getType();
    setType(type.getName());

    var schedule = dataSource.getSchedule();
    setDelay(schedule.getDelayMS());
    setRunAt(schedule.getRunAt());
    setRepeat(schedule.getRepeat());
    setRunOnWeekDays(schedule.getRunOnWeekDays());
    setRunOnMonths(schedule.getRunOnMonths());
    setDayOfMonth(schedule.getDayOfMonth());

    var license = dataSource.getLicense();
    setEnabled(dataSource.isEnabled() && schedule.isEnabled() && type.isEnabled() && license.isEnabled());
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
   * @return String return the type
   */
  public String getType() {
    return type;
  }

  /**
   * @param type the type to set
   */
  public void setType(String type) {
    this.type = type;
  }

  /**
   * @return int return the delay
   */
  public int getDelay() {
    return delay;
  }

  /**
   * @param delay the delay to set
   */
  public void setDelay(int delay) {
    this.delay = delay;
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
   * @return Date return the runAt
   */
  public Date getRunAt() {
    return runAt;
  }

  /**
   * @param runAt the runAt to set
   */
  public void setRunAt(Date runAt) {
    this.runAt = runAt;
  }

  /**
   * @return EnumSet{WeekDays} return the runOnWeekDays
   */
  public EnumSet<WeekDays> getRunOnWeekDays() {
    return runOnWeekDays;
  }

  /**
   * @param runOnWeekDays the runOnWeekDays to set
   */
  public void setRunOnWeekDays(EnumSet<WeekDays> runOnWeekDays) {
    this.runOnWeekDays = runOnWeekDays;
  }

  /**
   * @return EnumSet{Months} return the runOnMonths
   */
  public EnumSet<Months> getRunOnMonths() {
    return runOnMonths;
  }

  /**
   * @param runOnMonths the runOnMonths to set
   */
  public void setRunOnMonths(EnumSet<Months> runOnMonths) {
    this.runOnMonths = runOnMonths;
  }

  /**
   * @return int return the dayOfMonth
   */
  public int getDayOfMonth() {
    return dayOfMonth;
  }

  /**
   * @param dayOfMonth the dayOfMonth to set
   */
  public void setDayOfMonth(int dayOfMonth) {
    this.dayOfMonth = dayOfMonth;
  }

  /**
   * @return int return the repeat
   */
  public int getRepeat() {
    return repeat;
  }

  /**
   * @param repeat the repeat to set
   */
  public void setRepeat(int repeat) {
    this.repeat = repeat;
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

}
