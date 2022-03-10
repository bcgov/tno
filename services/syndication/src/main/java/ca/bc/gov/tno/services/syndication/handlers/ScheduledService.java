package ca.bc.gov.tno.services.syndication.handlers;

import java.util.List;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEvent;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataLocationService;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.dal.db.services.interfaces.IMediaTypeService;
import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.data.BaseDbScheduleService;
import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.data.events.TransactionBeginEvent;
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
  private final SyndicationConfig syndicationConfig;
  private final IMediaTypeService mediaTypeService;
  private final IDataLocationService dataLocationService;

  /**
   * Creates a new instance of a ScheduledService object, initializes with
   * specified parameters.
   * 
   * @param state               Service state.
   * @param syndicationConfig   Syndication media type config.
   * @param config              Syndication config.
   * @param dataSourceService   DAL DB data source service.
   * @param mediaTypeService    DAL DB media type service.
   * @param dataLocationService DAL DB data location service.
   * @param eventPublisher      Application event publisher.
   */
  @Autowired
  public ScheduledService(final ServiceState state,
      final SyndicationConfig syndicationConfig,
      final SyndicationCollectionConfig config,
      final IDataSourceService dataSourceService,
      final IMediaTypeService mediaTypeService,
      final IDataLocationService dataLocationService,
      final ApplicationEventPublisher eventPublisher) {
    super(state, config, dataSourceService, eventPublisher);
    this.syndicationConfig = syndicationConfig;
    this.mediaTypeService = mediaTypeService;
    this.dataLocationService = dataLocationService;
  }

  /**
   * Initialize the data source configurations.
   * There was no configuration provided, use the default for syndication
   * services.
   */
  @Override
  protected void initConfigs() {
    if (syndicationConfig == null)
      throw new IllegalArgumentException(
          "Argument 'syndicationConfig' in constructor is required and cannot be null.");

    var mediaTypeName = syndicationConfig.getMediaType();
    if (mediaTypeName == null || mediaTypeName.length() == 0)
      throw new IllegalArgumentException(
          "Argument 'syndicationConfig.mediaType' in constructor is required and cannot be null or empty.");

    if (mediaTypeService == null)
      throw new IllegalArgumentException("Argument 'mediaTypeService' in constructor is required and cannot be null.");

    var mediaType = mediaTypeService.findByName(mediaTypeName);

    if (!mediaType.isPresent())
      throw new IllegalArgumentException(
          String.format("Argument 'syndicationConfig.mediaType'='%s' does not exist in the data source.",
              mediaTypeName));

    // Fetch all data sources with the specified media type.
    List<DataSource> dataSources;

    var dataLocationName = syndicationConfig.getDataLocation();
    if (dataLocationName == null || dataLocationName.isEmpty()) {
      dataSources = dataSourceService.findByMediaTypeId(mediaType.get().getId());
    } else {
      var dataLocation = dataLocationService.findByName(dataLocationName);

      if (!dataLocation.isPresent())
        throw new IllegalArgumentException(
            String.format("Argument 'syndicationConfig.dataLocation'='%s' does not exist in the data source.",
                dataLocationName));

      dataSources = dataSourceService.findByMediaTypeIdAndDataLocationId(
          mediaType.get().getId(),
          dataLocation.get().getId());
    }

    // Only use the data sources that are configured.
    var approvedDataSources = dataSources.stream()
        .filter((ds) -> ds.isEnabled() && ds.getConnection().get("url") != null)
        .toList();
    approvedDataSources.forEach(ds -> sourceConfigs.getSources().add(new SyndicationConfig(ds)));
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   * 
   * @param dataSource The data source config.
   * @return The data source config.
   */
  @Override
  protected SyndicationConfig fetchDataSource(SyndicationConfig dataSource) {
    if (dataSource == null)
      throw new IllegalArgumentException("Parameter 'dataSource' is required.");

    var result = dataSourceService.findByCode(dataSource.getId());

    // If the database does not have a config for this source, then log warning.
    if (result.isEmpty()) {
      logger.warn(String.format("Data source '%s' does not exist in database", dataSource.getId()));
      return dataSource;
    }

    var newConfig = new SyndicationConfig(result.get());

    // TODO: Check for all conditions.
    if (dataSource.isEnabled() != newConfig.isEnabled()
        || !dataSource.getTopic().equals(newConfig.getTopic())
        || !dataSource.getMediaType().equals(newConfig.getMediaType()))
      logger.warn(String.format("Configuration has been changed for data source '%s'", dataSource.getId()));

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
  protected ApplicationEvent createEvent(Object source, SyndicationConfig dataSource, ScheduleConfig schedule) {
    return new TransactionBeginEvent(source, dataSource, schedule);
  }
}