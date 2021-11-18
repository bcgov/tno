package ca.bc.gov.tno.services.data.config;

import java.util.ArrayList;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for Data Sources.
 * 
 * @param <T> The data source config type.
 */
@Configuration
@ConfigurationProperties("data")
public class DataSourceCollectionConfig<T extends DataSourceConfig> {

  /**
   * An array of data source configurations.
   */
  private ArrayList<T> sources = new ArrayList<T>();

  /**
   * @return ArrayList{T} return the sources
   */
  public ArrayList<T> getSources() {
    return sources;
  }

  /**
   * @param sources the sources to set
   */
  public void setSources(ArrayList<T> sources) {
    this.sources = sources;
  }

}
