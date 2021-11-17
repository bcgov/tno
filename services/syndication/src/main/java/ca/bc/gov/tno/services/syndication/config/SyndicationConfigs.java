package ca.bc.gov.tno.services.syndication.config;

import java.util.ArrayList;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for the default Syndication Feeds. These values will
 * be used if a connection to the database cannot be made.
 */
@Configuration
@ConfigurationProperties("data")
public class SyndicationConfigs extends DataSourceConfig {
  /**
   * 
   */
  private ArrayList<SyndicationConfig> sources = new ArrayList<SyndicationConfig>();

  /**
   * @return ArrayList<SyndicationConfig> return the sources
   */
  public ArrayList<SyndicationConfig> getSources() {
    return sources;
  }

  /**
   * @param sources the sources to set
   */
  public void setSources(ArrayList<SyndicationConfig> sources) {
    this.sources = sources;
  }

}
