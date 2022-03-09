package ca.bc.gov.tno.services.capture.config;

import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;

import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for the default Capture service. These values will
 * be used if a connection to the database cannot be made.
 */
@Configuration
public class CaptureCollectionConfig extends DataSourceCollectionConfig<CaptureConfig> {

}
