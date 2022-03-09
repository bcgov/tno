package ca.bc.gov.tno.services.capture.handlers;

import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.Date;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;
import org.springframework.context.ApplicationEvent;

import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.dal.db.services.interfaces.IMediaTypeService;
import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.data.BaseDbScheduleService;
import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.data.events.TransactionBeginEvent;
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
  private final IMediaTypeService mediaTypeService;

  /**
   * Creates a new instance of a ScheduledService object, initializes with
   * specified parameters.
   * 
   * @param state             Service state.
   * @param mediaConfig       Capture media type config.
   * @param config            Capture config.
   * @param dataSourceService DAL DB data source service.
   * @param mediaTypeService  DAL DB media type service.
   * @param eventPublisher    Application event publisher.
   */
  @Autowired
  public ScheduledService(final ServiceState state,
      final CaptureConfig mediaConfig,
      final CaptureCollectionConfig config,
      final IDataSourceService dataSourceService,
      final IMediaTypeService mediaTypeService,
      final ApplicationEventPublisher eventPublisher) {
    super(state, config, dataSourceService, eventPublisher);
    this.mediaConfig = mediaConfig;
    this.mediaTypeService = mediaTypeService;
  }

  /**
   * Update the data source with the current ranAt date and time.
   * 
   * @param config The data source config.
   * @param ranAt  The date and time the transaction ran at.
   */
  @Override
  protected void updateDataSource(DataSourceConfig dataSource, ScheduleConfig schedule, ZonedDateTime ranOn) {
    super.updateDataSource(dataSource, schedule, ranOn);

    var result = dataSourceService.findByCode(dataSource.getId());
    if (result.isPresent()) {
      var captureSource = result.get();
      var connection = captureSource.getConnection();
      connection.put("streamStart", ((CaptureConfig) dataSource).getStreamStartTime());
      captureSource.setConnection(connection);
      dataSourceService.update(captureSource);
    }
  }

  /**
   * Initialize the data source configurations.
   * There was no configuration provided, use the default for capture
   * services.
   */
  @Override
  protected void initConfigs() {
    if (mediaConfig == null)
      throw new IllegalArgumentException(
          "Argument 'mediaConfig' in constructor is required and cannot be null.");

    var mediaTypeName = mediaConfig.getMediaType();
    if (mediaTypeName == null || mediaTypeName.length() == 0)
      throw new IllegalArgumentException(
          "Argument 'mediaConfig.mediaType' in constructor is required and cannot be null or empty.");

    if (mediaTypeService == null)
      throw new IllegalArgumentException("Argument 'mediaTypeService' in constructor is required and cannot be null.");

    var mediaType = mediaTypeService.findByName(mediaTypeName);

    if (!mediaType.isPresent())
      throw new IllegalArgumentException(
          String.format("Argument 'mediaConfig.mediaType'='%s' does not exist in the data source.", mediaTypeName));

    // Fetch all data sources with the specified media type.
    var dataSources = dataSourceService.findByMediaTypeId(mediaType.get().getId());
    dataSources.forEach(ds -> sourceConfigs.getSources().add(new CaptureConfig(ds)));
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   * 
   * @param config The data source config.
   * @return The data source config.
   */
  @Override
  protected CaptureConfig fetchDataSource(CaptureConfig config) {
    if (config == null)
      throw new IllegalArgumentException("Parameter 'config' is required.");

    var result = dataSourceService.findByCode(config.getId());

    // If the database does not have a config for this source, then log warning.
    if (result.isEmpty()) {
      logger.warn(String.format("Data source '%s' does not exist in database", config.getId()));
      return config;
    }

    var newConfig = new CaptureConfig(result.get());

    // TODO: Check for all conditions.
    if (config.isEnabled() != newConfig.isEnabled()
        || !config.getTopic().equals(newConfig.getTopic())
        || !config.getMediaType().equals(newConfig.getMediaType()))
      logger.warn(String.format("Configuration has been changed for data source '%s'", config.getId()));

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
