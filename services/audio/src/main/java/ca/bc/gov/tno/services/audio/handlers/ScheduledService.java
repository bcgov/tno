package ca.bc.gov.tno.services.audio.handlers;

import java.time.LocalDate;
import java.time.ZoneId;
import java.time.ZonedDateTime;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEvent;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.services.ScheduleService;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.dal.db.services.interfaces.IMediaTypeService;
import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.data.BaseDbScheduleService;
import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.data.events.TransactionBeginEvent;
import ca.bc.gov.tno.services.audio.config.AudioConfig;
import ca.bc.gov.tno.services.audio.config.AudioCollectionConfig;

/**
 * ScheduledService class, provides a process that manages the scheduling of
 * ingestion. This will ensure the process runs according to the configured
 * schedule.
 */
@Async
@Component
public class ScheduledService
extends BaseDbScheduleService<AudioConfig, DataSourceCollectionConfig<AudioConfig>> {

  private static final Logger logger = LogManager.getLogger(ScheduledService.class);
  private final AudioConfig mediaConfig;
  private final IMediaTypeService mediaTypeService;
  private final ScheduleService scheduleService;

  /**
   * Creates a new instance of a ScheduledService object, initializes with
   * specified parameters.
   *
   * @param state             Service state.
   * @param mediaConfig       Audio media type config.
   * @param config            Audio config.
   * @param dataSourceService DAL DB data source service.
   * @param mediaTypeService  DAL DB media type service.
   * @param eventPublisher    Application event publisher.
   */
  @Autowired
  public ScheduledService(final ServiceState state,
      final AudioConfig mediaConfig,
      final AudioCollectionConfig config,
      final IDataSourceService dataSourceService,
      final IMediaTypeService mediaTypeService,
      final ApplicationEventPublisher eventPublisher,
      final ScheduleService scheduleService) {
    super(state, config, dataSourceService, eventPublisher);
    this.mediaConfig = mediaConfig;
    this.mediaTypeService = mediaTypeService;
    this.scheduleService = scheduleService;
  }

    /**
   * Update the data source with the next run_on date and time (one day from now).
   *
   * For clips, because the audio being extracted must already have been recorded, the clip process cannot begin until
   * the clip's stop time. This is the time at which this schedule will trigger. It is also the time at which the next
   * run time for this clip will be set when clip extraction is completed.
   *
   * The stop time is recorded in the database as the number of milliseconds since midnight (time only), while the next
   * run time is a full date/time. To calculate the next run time we need the time at the previous midnight. This is then
   * added to the stop time and TimeUnit.DAYS.toMillis(1) to determine the next run time for this schedule.
   *
   * @param config The data source config.
   * @param ranOn  The date and time the transaction ran at.
   */
  @Override
  protected void updateDataSource(DataSourceConfig dataSource, ScheduleConfig schedule, ZonedDateTime ranOn) {
    var next = schedule.getStopAt().atDate(LocalDate.now()).plusDays(1);
    var sched = scheduleService.findById(schedule.getId()).get();
    sched.setRunOn(next.atZone(ZoneId.of(mediaConfig.getTimezone())));
    scheduleService.update(sched);
    super.updateDataSource(dataSource, schedule, ranOn);
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
    dataSources.forEach(ds -> sourceConfigs.getSources().add(new AudioConfig(ds, dataSourceService)));
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   *
   * @param config The data source config.
   * @return The data source config.
   */
  @Override
  protected AudioConfig fetchDataSource(AudioConfig config) {
    if (config == null)
      throw new IllegalArgumentException("Parameter 'config' is required.");

    var result = dataSourceService.findByCode(config.getId());

    // If the database does not have a config for this source, then log warning.
    if (result.isEmpty()) {
      logger.warn(String.format("Data source '%s' does not exist in database", config.getId()));
      return config;
    }

    var newConfig = new AudioConfig(result.get(), dataSourceService);

    // TODO: Check for all conditions.
    if (config.getIsEnabled() != newConfig.getIsEnabled()
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
  protected ApplicationEvent createEvent(Object source, AudioConfig dataSource, ScheduleConfig schedule) {
    return new TransactionBeginEvent(source, dataSource, schedule);
  }
}
