package ca.bc.gov.tno.services.syndication.config;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;

/**
 * Configuration settings for the default Syndication Feeds. These values will
 * be used if a connection to the database cannot be made.
 */
@Configuration
@ConfigurationProperties("data")
public class SyndicationCollectionConfig extends DataSourceCollectionConfig<SyndicationConfig> {

}
