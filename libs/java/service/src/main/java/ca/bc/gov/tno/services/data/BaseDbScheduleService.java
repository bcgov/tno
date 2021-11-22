package ca.bc.gov.tno.services.data;

import java.util.Date;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;
import ca.bc.gov.tno.services.ServiceState;

/**
 * BaseDbScheduleService class, provides a process that manages the scheduling
 * of ingestion. This will ensure the process runs according to the configured
 * schedule.
 * 
 * @param <C>  The data source config type.
 * @param <CA> The data source config collection type.
 */
@Async
public abstract class BaseDbScheduleService<C extends DataSourceConfig, CA extends DataSourceCollectionConfig<C>>
    extends BaseScheduleService<C, CA> {
  private static final Logger logger = LogManager.getLogger(BaseDbScheduleService.class);

  /**
   * The data source service.
   */
  protected final IDataSourceService dataSourceService;

  /**
   * Creates a new instance of a BaseDbScheduleService object, initializes with
   * specified parameters.
   * 
   * @param state             Service state.
   * @param config            Data source config.
   * @param dataSourceService DAL DB data source service.
   * @param eventPublisher    Application event publisher.
   */
  public BaseDbScheduleService(final ServiceState state, final CA config, final IDataSourceService dataSourceService,
      final ApplicationEventPublisher eventPublisher) {
    super(state, config, eventPublisher);
    this.dataSourceService = dataSourceService;
  }

  /**
   * Initialize the data source configurations.
   */
  @Override
  @SuppressWarnings("unchecked")
  protected void initConfigs() {
    var dataSources = dataSourceService.findAll();
    dataSources.forEach(ds -> sourceConfigs.getSources().add((C) new DataSourceConfig(ds)));
  }

  /**
   * Update the data source with the current ranAt date and time.
   * 
   * @param config The data source config.
   * @param ranAt  The date and time the transaction ran at.
   */
  @Override
  protected void updateConfig(C config, Date ranAt) {
    super.updateConfig(config, ranAt);

    var result = dataSourceService.findByCode(config.getId());
    if (result.isPresent()) {
      var dataSource = result.get();
      dataSource.setLastRanOn(ranAt);
      dataSourceService.update(dataSource);
    }
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   * 
   * @param config The data source config.
   * @return The data source config.
   */
  @Override
  protected C fetchConfig(C config) {
    if (config == null)
      throw new IllegalArgumentException("Parameter 'config' is required.");

    var result = dataSourceService.findByCode(config.getId());

    // If the database does not have a config for this source, then log warning.
    if (result.isEmpty()) {
      logger.warn(String.format("Data source '%s' does not exist in database", config.getId()));
      return config;
    }

    @SuppressWarnings("unchecked")
    var newConfig = (C) new DataSourceConfig(result.get());

    // TODO: Check for all conditions.
    if (config.isEnabled() != newConfig.isEnabled() || !config.getTopic().equals(newConfig.getTopic())
        || !config.getType().equals(newConfig.getType()) || config.getDelay() != newConfig.getDelay()
        || !config.getRunAt().equals(newConfig.getRunAt()) || config.getRepeat() != newConfig.getRepeat())
      logger.warn(String.format("Configuration has been changed for data source '%s'", config.getId()));

    return newConfig;
  }
}
