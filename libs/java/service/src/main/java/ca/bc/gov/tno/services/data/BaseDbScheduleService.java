package ca.bc.gov.tno.services.data;

import java.time.ZonedDateTime;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.scheduling.annotation.Async;
import org.springframework.web.client.ResourceAccessException;

import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;

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

  /** AJAX Requester */
  protected final TnoApi api;

  /**
   * Creates a new instance of a BaseDbScheduleService object, initializes with
   * specified parameters.
   *
   * @param state          Service state.
   * @param config         Data source config.
   * @param eventPublisher Application event publisher.
   * @param dataSourceApi  DataSource API.
   */
  public BaseDbScheduleService(final ServiceState state, final CA config,
      final ApplicationEventPublisher eventPublisher, final TnoApi dataSourceApi) {
    super(state, config, eventPublisher);
    this.api = dataSourceApi;
  }

  /**
   * Update the data source with the current ranAt date and time.
   *
   * @param config   The data source config.
   * @param schedule The schedule config.
   * @param ranOn    The date and time the transaction ran at.
   * @throws ApiException A failure occurred communicating with the api.
   */
  @Override
  protected void updateDataSource(DataSourceConfig config, ScheduleConfig schedule, ZonedDateTime ranOn)
      throws ApiException {
    super.updateDataSource(config, schedule, ranOn);

    var dataSource = api.getDataSource(config.getId());
    if (dataSource != null) {
      dataSource.setLastRanOn(ranOn);
      dataSource.setFailedAttempts(config.getFailedAttempts());
      api.update(dataSource);
    }
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   *
   * @param dataSourceConfig The data source config.
   * @return The data source config.
   * @throws ApiException A failure occurred communicating with the api.
   */
  @Override
  protected C fetchDataSource(C dataSourceConfig) throws ApiException, ResourceAccessException {
    if (dataSourceConfig == null)
      throw new IllegalArgumentException("Parameter 'dataSourceConfig' is required.");

    var dataSource = api.getDataSource(dataSourceConfig.getId());

    if (dataSource == null)
      throw new ApiException(String.format("Data-source does not exist '%s'", dataSourceConfig.getId()));

    @SuppressWarnings("unchecked")
    var newConfig = (C) new DataSourceConfig(dataSource); // TODO: Java is horrible and this doesn't work right. Need to
                                                          // override in child class.

    // TODO: Check for all conditions.
    if (dataSourceConfig.getIsEnabled() != newConfig.getIsEnabled()
        || !dataSourceConfig.getTopic().equals(newConfig.getTopic())
        || !dataSourceConfig.getMediaType().equals(newConfig.getMediaType()))
      logger.warn(String.format("Configuration has been changed for data-source '%s'", dataSourceConfig.getId()));

    return newConfig;
  }
}
