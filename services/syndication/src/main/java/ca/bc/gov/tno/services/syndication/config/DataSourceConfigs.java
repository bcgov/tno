package ca.bc.gov.tno.services.syndication.config;

import java.util.ArrayList;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for Data Sources.
 */
@Configuration
@ConfigurationProperties("data")
public class DataSourceConfigs {

  private ArrayList<DataSourceConfig> sources = new ArrayList<DataSourceConfig>();

  /**
   * @return ArrayList<DataSourceConfig> return the sources
   */
  public ArrayList<DataSourceConfig> getSources() {
    return sources;
  }

  /**
   * @param sources the sources to set
   */
  public void setSources(ArrayList<DataSourceConfig> sources) {
    this.sources = sources;
  }

}
