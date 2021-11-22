package ca.bc.gov.tno.services.syndication.config;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;

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
    super(dataSource);

    setUrl((String) dataSource.getConnection().get("url"));
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

}
