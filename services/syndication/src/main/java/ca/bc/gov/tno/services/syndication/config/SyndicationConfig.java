package ca.bc.gov.tno.services.syndication.config;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.dal.db.entities.DataSource;

/**
 * Configuration settings for the default Syndication Feed. These values will be
 * used if a connection to the database cannot be made.
 */
@Configuration
@ConfigurationProperties("data.source")
public class SyndicationConfig extends DataSourceConfig {
  /**
   * URL to the syndication data source feed.
   */
  private String url;

  /**
   * Number of times this data source has been run. This value is compared to the
   * 'repeat' configuration.
   */
  private int ranCounter;

  /**
   * Creates a new instance of a SyndicationConfig object.
   */
  public SyndicationConfig() {

  }

  /**
   * Creates a new instance of a SyndicationConfig object, initializes with
   * specified parameters.
   * 
   * @param dataSource
   */
  public SyndicationConfig(DataSource dataSource) {

    setId(dataSource.getCode());
    setTopic(dataSource.getTopic());
    setLastRanOn(dataSource.getLastRanOn());
    setUrl((String) dataSource.getConnection().get("url"));

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
   * @return String return the url
   */
  public String getUrl() {
    return url;
  }

  /**
   * @param url the url to set
   */
  public void setUrl(String url) {
    this.url = url;
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
