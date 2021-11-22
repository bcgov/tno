package ca.bc.gov.tno.services.syndication.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.data.BaseDbScheduleService;
import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;
import ca.bc.gov.tno.services.syndication.config.SyndicationConfig;
import ca.bc.gov.tno.services.syndication.config.SyndicationCollectionConfig;

/**
 * ScheduledService class, provides a process that manages the scheduling of
 * ingestion. This will ensure the process runs according to the configured
 * schedule.
 */
@Async
@Component
public class ScheduledService
    extends BaseDbScheduleService<SyndicationConfig, DataSourceCollectionConfig<SyndicationConfig>> {
  private static final Logger logger = LogManager.getLogger(ScheduledService.class);

  /**
   * Creates a new instance of a ScheduledService object, initializes with
   * specified parameters.
   * 
   * @param state             Service state.
   * @param config            Syndication config.
   * @param dataSourceService DAL DB data source service.
   * @param eventPublisher    Application event publisher.
   */
  @Autowired
  public ScheduledService(final ServiceState state, final SyndicationCollectionConfig config,
      final IDataSourceService dataSourceService, final ApplicationEventPublisher eventPublisher) {
    super(state, config, dataSourceService, eventPublisher);
  }

  /**
   * Initialize the data source configurations.
   */
  @Override
  protected void initConfigs() {
    // TODO: Fetch syndication sources from database.
    var dataSources = dataSourceService.findAll();
    dataSources.forEach(ds -> sourceConfigs.getSources().add(new SyndicationConfig(ds)));
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   * 
   * @param config The data source config.
   * @return The data source config.
   */
  @Override
  protected SyndicationConfig fetchConfig(SyndicationConfig config) {
    if (config == null)
      throw new IllegalArgumentException("Parameter 'config' is required.");

    var result = dataSourceService.findByCode(config.getId());

    // If the database does not have a config for this source, then log warning.
    if (result.isEmpty()) {
      logger.warn(String.format("Data source '%s' does not exist in database", config.getId()));
      return config;
    }

    var newConfig = new SyndicationConfig(result.get());

    // TODO: Check for all conditions.
    if (config.isEnabled() != newConfig.isEnabled() || !config.getTopic().equals(newConfig.getTopic())
        || !config.getType().equals(newConfig.getType()) || config.getDelay() != newConfig.getDelay()
        || !config.getRunAt().equals(newConfig.getRunAt()) || config.getRepeat() != newConfig.getRepeat())
      logger.warn(String.format("Configuration has been changed for data source '%s'", config.getId()));

    return newConfig;
  }
}
