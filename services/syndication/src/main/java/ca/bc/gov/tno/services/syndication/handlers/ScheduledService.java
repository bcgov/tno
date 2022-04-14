package ca.bc.gov.tno.services.syndication.handlers;

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
  private final SyndicationConfig syndicationConfig;

  private static final Logger logger = LogManager.getLogger(ScheduledService.class);

  /**
   * Creates a new instance of a ScheduledService object, initializes with
   * specified parameters.
   *
   * @param state             Service state.
   * @param syndicationConfig Syndication media type config.
   * @param config            Syndication config.
   * @param eventPublisher    Application event publisher.
   * @param api               Data source API.
   */
  @Autowired
  public ScheduledService(final ServiceState state,
      final SyndicationConfig syndicationConfig,
      final SyndicationCollectionConfig config,
      final ApplicationEventPublisher eventPublisher,
      final TnoApi api) {
    super(state, config, eventPublisher, api);
    this.syndicationConfig = syndicationConfig;
  }

  /**
   * Initialize the data source configurations.
   * There was no configuration provided, use the default for syndication
   * services.
   */
  @Override
  protected void initConfigs() throws ApiException {
    if (syndicationConfig == null)
      throw new IllegalArgumentException(
          "Argument 'syndicationConfig' in constructor is required and cannot be null.");

    var mediaTypeName = syndicationConfig.getMediaType();
    if (mediaTypeName == null || mediaTypeName.length() == 0)
      throw new IllegalArgumentException(
          "Argument 'syndicationConfig.mediaType' in constructor is required and cannot be null or empty.");

    if (api == null)
      throw new IllegalArgumentException("Argument 'api' in constructor is required and cannot be null.");

    // Fetch all data sources with the specified media type.
    var dataSources = api.getDataSourcesForMediaType(mediaTypeName);

    // Only use the data sources that are configured.
    var approvedDataSources = dataSources.stream()
        .filter((ds) -> ds.getIsEnabled() && ds.getConnection().get("url") != null)
        .toList();
    approvedDataSources.forEach(ds -> sourceConfigs.getSources().add(new SyndicationConfig(ds)));
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   *
   * @param config The data source config.
   * @return The data source config.
   * @throws ApiException            A failure occurred communicating with the
   *                                 api.
   * @throws ResourceAccessException A failure occurred with the AJAX request.
   */
  @Override
  protected SyndicationConfig fetchDataSource(SyndicationConfig config) throws ResourceAccessException, ApiException {
    if (config == null)
      throw new IllegalArgumentException("Parameter 'dataSourceConfig' is required.");

    var dataSource = api.getDataSource(config.getId());

    if (dataSource == null)
      throw new ApiException(String.format("Data-source does not exist '%s'", config.getId()));

    var newConfig = new SyndicationConfig(dataSource);

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
  protected ApplicationEvent createEvent(Object source, SyndicationConfig dataSource, ScheduleConfig schedule) {
    return new TransactionBeginEvent(source, dataSource, schedule);
  }
}
