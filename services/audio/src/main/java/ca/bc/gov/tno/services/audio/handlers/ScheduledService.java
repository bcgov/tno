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
import org.springframework.web.client.ResourceAccessException;

import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.data.ApiException;
import ca.bc.gov.tno.services.data.BaseDbScheduleService;
import ca.bc.gov.tno.services.data.TnoApi;
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

  /**
   * Creates a new instance of a ScheduledService object, initializes with
   * specified parameters.
   *
   * @param state          Service state.
   * @param mediaConfig    Audio media type config.
   * @param config         Audio config.
   * @param api            DAL DB data source service.
   * @param eventPublisher Application event publisher.
   */
  @Autowired
  public ScheduledService(final ServiceState state,
      final AudioConfig mediaConfig,
      final AudioCollectionConfig config,
      final TnoApi api,
      final ApplicationEventPublisher eventPublisher) {
    super(state, config, eventPublisher, api);
    this.mediaConfig = mediaConfig;
  }

  /**
   * Update the data source with the next run_on date and time (one day from now).
   *
   * For clips, because the audio being extracted must already have been recorded,
   * the clip process cannot begin until
   * the clip's stop time. This is the time at which this schedule will trigger.
   * It is also the time at which the next
   * run time for this clip will be set when clip extraction is completed.
   *
   * The stop time is recorded in the database as the number of milliseconds since
   * midnight (time only), while the next
   * run time is a full date/time. To calculate the next run time we need the time
   * at the previous midnight. This is then
   * added to the stop time and TimeUnit.DAYS.toMillis(1) to determine the next
   * run time for this schedule.
   *
   * @param config The data source config.
   * @param ranOn  The date and time the transaction ran at.
   * @throws ApiException
   */
  @Override
  protected void updateDataSource(DataSourceConfig config, ScheduleConfig schedule, ZonedDateTime ranOn)
      throws ApiException {
    // TODO: The schedule should not be modified by the service. The schedule
    // informs the service what to do, not the other way around.
    // This needs to be redesigned.
    var next = schedule.getStopAt().atDate(LocalDate.now()).plusDays(1);
    var sched = config.getSchedules().stream().filter(s -> s.getId() == schedule.getId()).findFirst().get();
    sched.setRunOn(next.atZone(ZoneId.of(mediaConfig.getTimezone())));
    super.updateDataSource(config, schedule, ranOn);
  }

  /**
   * Initialize the data source configurations.
   * There was no configuration provided, use the default for capture
   * services.
   * 
   * @throws ApiException
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
        .filter((ds) -> ds.getIsEnabled() && ds.getParentId() != null)
        .toList();
    approvedDataSources.forEach(ds -> sourceConfigs.getSources().add(new AudioConfig(ds)));
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   *
   * @param config The data source config.
   * @return The data source config.
   * @throws ApiException
   * @throws ResourceAccessException
   */
  @Override
  protected AudioConfig fetchDataSource(AudioConfig config) throws ResourceAccessException, ApiException {
    if (config == null)
      throw new IllegalArgumentException("Parameter 'dataSourceConfig' is required.");

    var dataSource = api.getDataSource(config.getId());

    if (dataSource == null)
      throw new ApiException(String.format("Data-source does not exist '%s'", config.getId()));

    if (dataSource.getParent() == null)
      throw new ApiException(String.format("Data-source parent does not exist '%s'", config.getId()));

    var newConfig = new AudioConfig(dataSource);

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
  protected ApplicationEvent createEvent(Object source, AudioConfig dataSource, ScheduleConfig schedule) {
    return new TransactionBeginEvent(source, dataSource, schedule);
  }
}
