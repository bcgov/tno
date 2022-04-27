package ca.bc.gov.tno.services.capture.handlers;

import java.time.ZonedDateTime;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;
import org.springframework.web.client.ResourceAccessException;
import org.springframework.context.ApplicationEvent;

import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.data.ApiException;
import ca.bc.gov.tno.services.data.BaseDbScheduleService;
import ca.bc.gov.tno.services.data.TnoApi;
import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.data.events.TransactionBeginEvent;
import ca.bc.gov.tno.services.handlers.ErrorHandler;
import ca.bc.gov.tno.services.capture.config.CaptureConfig;
import ca.bc.gov.tno.services.capture.config.CaptureCollectionConfig;

/**
 * ScheduledService class, provides a process that manages the scheduling of
 * ingestion. This will ensure the process runs according to the configured
 * schedule.
 */
@Async
@Component
public class ScheduledService
    extends BaseDbScheduleService<CaptureConfig, DataSourceCollectionConfig<CaptureConfig>> {

  private static final Logger logger = LogManager.getLogger(ScheduledService.class);
  private final CaptureConfig mediaConfig;

  /**
   * Creates a new instance of a ScheduledService object, initializes with
   * specified parameters.
   *
   * @param state          Service state.
   * @param mediaConfig    Capture media type config.
   * @param config         Capture config.
   * @param api            DAL DB data source service.
   * @param eventPublisher Application event publisher.
   */
  @Autowired
  public ScheduledService(final ServiceState state,
      final CaptureConfig mediaConfig,
      final CaptureCollectionConfig config,
      final TnoApi api,
      final ApplicationEventPublisher eventPublisher) {
    super(state, config, eventPublisher, api);
    this.mediaConfig = mediaConfig;
  }

  /**
   * Update the data source with the current ranAt date and time.
   *
   * @param config The data source config.
   * @param ranAt  The date and time the transaction ran at.
   * @throws ApiException
   */
  @Override
  protected void updateDataSource(DataSourceConfig config, ScheduleConfig schedule, ZonedDateTime ranOn)
      throws ApiException {
    super.updateDataSource(config, schedule, ranOn);

    var dataSource = api.getDataSource(config.getId());
    if (dataSource != null) {
      dataSource.setFailedAttempts(config.getFailedAttempts());
      var connection = dataSource.getConnection();
      connection.put("streamStart", ((CaptureConfig) config).getStreamStartTime());
      connection.put("runningNow", ((CaptureConfig) config).getRunningCommand());
      dataSource.setConnection(connection);
      api.update(dataSource);
    }
  }

  /**
   * Initialize the data source configurations.
   * There was no configuration provided, use the default for capture
   * services.
   */
  @Override
  protected void initConfigs() throws ApiException {
    if (mediaConfig == null)
      throw new IllegalArgumentException(
          "Argument 'syndicationConfig' in constructor is required and cannot be null.");

    var mediaTypeName = mediaConfig.getMediaType();
    if (mediaTypeName == null || mediaTypeName.length() == 0)
      throw new IllegalArgumentException(
          "Argument 'syndicationConfig.mediaType' in constructor is required and cannot be null or empty.");

    if (api == null)
      throw new IllegalArgumentException("Argument 'api' in constructor is required and cannot be null.");

    // Fetch all data sources with the specified media type.
    var dataSources = api.getDataSourcesForMediaType(mediaTypeName);

    // Only use the data sources that are configured.
    var approvedDataSources = dataSources.stream()
        .filter((ds) -> ds.getIsEnabled() &&
            ds.getConnection().get("url") != null)
        .toList();
      
    for (var ds : approvedDataSources) {
      sourceConfigs.getSources().add(new CaptureConfig(ds));
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
  protected CaptureConfig fetchDataSource(CaptureConfig config) throws ResourceAccessException, ApiException {
    if (config == null)
      throw new IllegalArgumentException("Parameter 'dataSourceConfig' is required.");

    var dataSource = api.getDataSource(config.getId());

    if (dataSource == null)
      throw new ApiException(String.format("Data-source does not exist '%s'", config.getId()));

    var newConfig = new CaptureConfig(dataSource);

    // TODO: Check for all conditions.
    if (config.getIsEnabled() != newConfig.getIsEnabled()
        || !config.getTopic().equals(newConfig.getTopic())
        || !config.getMediaType().equals(newConfig.getMediaType()))
      logger.warn(String.format("Configuration has been changed for data-source '%s'", config.getId()));

    return newConfig;
  }

  /**
   * Create the event that the scheduler will publish.
   *
   * @param dataSource The data source config.
   * @param schedule   The schedule config.
   * @return A new application event.
   */
  @Override
  protected ApplicationEvent createEvent(Object source, CaptureConfig dataSource, ScheduleConfig schedule) {
    return new TransactionBeginEvent(source, dataSource, schedule);
  }
}
