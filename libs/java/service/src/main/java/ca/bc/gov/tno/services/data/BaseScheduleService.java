package ca.bc.gov.tno.services.data;

import java.time.ZonedDateTime;
import java.util.UUID;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.context.ApplicationEvent;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.EventListener;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.events.ServiceStartEvent;
import ca.bc.gov.tno.services.data.config.DataSourceConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.data.config.DataSourceCollectionConfig;
import ca.bc.gov.tno.services.data.events.TransactionCompleteEvent;

/**
 * BaseScheduleService class, provides a process that manages the scheduling of
 * ingestion. This will ensure the process runs according to the configured
 * schedule.
 * 
 * @param <C>  The data source config type.
 * @param <CA> The data source config collection type.
 */
@Async
public abstract class BaseScheduleService<C extends DataSourceConfig, CA extends DataSourceCollectionConfig<C>>
    implements ApplicationListener<ServiceStartEvent> {
  private static final Logger logger = LogManager.getLogger(BaseScheduleService.class);

  /**
   * Unique id for this schedule service.
   */
  private final UUID uid;

  /**
   * The data source collection configuration.
   */
  protected final CA sourceConfigs;

  /**
   * The application event publisher.
   */
  protected final ApplicationEventPublisher eventPublisher;

  /**
   * The service state.
   */
  protected final ServiceState state;

  /**
   * Creates a new instance of a BaseScheduleService object, initializes with
   * specified parameters.
   * 
   * @param state          Service state.
   * @param configs        Data source config.
   * @param eventPublisher Application event publisher.
   */
  public BaseScheduleService(final ServiceState state, final CA configs,
      final ApplicationEventPublisher eventPublisher) {
    this.uid = UUID.randomUUID();
    this.state = state;
    this.sourceConfigs = configs;
    this.eventPublisher = eventPublisher;
  }

  /**
   * Get the GUID to identify this scheduler.
   * 
   * @return GUID of this scheduler.
   */
  public UUID getId() {
    return uid;
  }

  /**
   * When a done event occurs inform the scheduler to continue.
   * 
   * @param event The event.
   */
  @EventListener
  public void handleTransactionCompleteEvent(TransactionCompleteEvent event) {
    var source = event.getSource();
    if (ScheduleHelper.isSchedule(source)) {
      @SuppressWarnings("unchecked")
      var caller = (BaseScheduleService<C, CA>) event.getSource();
      if (caller.getId() == this.uid) {
        logger.info("Transaction complete event.");
        var dataSource = event.getDataSource();
        var schedule = event.getSchedule();

        try {
          updateDataSource(dataSource, schedule, ZonedDateTime.now());
          // Reset failures as the source has been completed 'successfully'.
          state.setFailedAttempts(0);
        } catch (Exception ex) {
          // If an error occurs we don't want to block the current process, but we do want
          // to log it.
          var errorEvent = new ErrorEvent(this, ex);
          eventPublisher.publishEvent(errorEvent);
        }
        synchronized (this) {
          notify();
        }
      }
    }
  }

  /**
   * When the start event occurs run the scheduler.
   * Based on the schedule keep checking for new content
   * from the data source.
   * 
   * @param event The event.
   */
  @Override
  public void onApplicationEvent(ServiceStartEvent event) {
    try {
      logger.info(String.format("Scheduler started: id: %s", uid));

      // Fetch syndication data sources from TNO DB.
      if (sourceConfigs.getSources().isEmpty()) {
        initConfigs();
      }

      var index = 0;
      state.setStatus(ServiceStatus.running);

      while (state.getStatus() != ServiceStatus.sleeping) {
        if (state.getStatus() == ServiceStatus.pause) {
          state.setStatus(ServiceStatus.paused);
        } else if (state.getStatus() == ServiceStatus.resume) {
          state.setStatus(ServiceStatus.running);
        }

        if (state.getStatus() == ServiceStatus.running) {
          // Reset index to start over.
          if (index == sourceConfigs.getSources().size())
            index = 0;

          if (sourceConfigs.getSources().size() == 0) {
            state.setStatus(ServiceStatus.sleeping);
            var errorEvent = new ErrorEvent(this, new Exception("There are no configured data sources"));
            eventPublisher.publishEvent(errorEvent);
            continue;
          }

          var dataSource = (C) sourceConfigs.getSources().get(index);
          // Make request to TNO DB for data source configuration settings.
          dataSource = fetchDataSource(dataSource);

          // Determine if the data source should be imported based on the configured
          // schedule.
          for (ScheduleConfig schedule : dataSource.getSchedules()) {
            if (ScheduleHelper.verifySchedule(ZonedDateTime.now(), dataSource, schedule)) {
              logger.debug(String.format("Data source start: %s", dataSource.getId()));
              var beginEvent = createEvent(this, dataSource, schedule);
              // var beginEvent = new TransactionBeginEvent<C>(this, dataSource, schedule);
              eventPublisher.publishEvent(beginEvent);

              // TODO: Remove synchronous limitations.
              // Wait until the SourceCompleteEvent is received.
              synchronized (this) {
                wait();
              }
            }
          }

          index++;
        }

        // Probably not needed, but didn't want a run away process. Which can occur
        // during pause, or if all syndication sources are disabled.
        Thread.sleep(50);
      }
    } catch (InterruptedException ex) {
      state.setStatus(ServiceStatus.sleeping);
      var errorEvent = new ErrorEvent(this, ex);
      eventPublisher.publishEvent(errorEvent);
    }
  }

  /**
   * Create the event that the scheduler will publish.
   * 
   * @param source     The source of the event.
   * @param dataSource The data source config.
   * @param schedule   The schedule config.
   * @return A new application event.
   */
  protected abstract ApplicationEvent createEvent(Object source, C dataSource, ScheduleConfig schedule);

  /**
   * Initialize the data source configurations.
   */
  protected void initConfigs() {
  }

  /**
   * Make a request to the TNO DB to fetch an updated configuration. If none is
   * found, return the current config. Log if the config is different.
   * 
   * @param dataSource The data source config.
   * @return The data source config.
   */
  protected C fetchDataSource(C dataSource) {
    return dataSource;
  }

  /**
   * Update the data source with the current ranAt date and time.
   * 
   * @param dataSource The data source config.
   * @param schedule   The schedule config.
   * @param ranOn      The date and time the transaction ran.
   */
  protected void updateDataSource(DataSourceConfig dataSource, ScheduleConfig schedule, ZonedDateTime ranOn) {
    if (dataSource == null)
      throw new IllegalArgumentException("Parameter 'dataSource' is required.");
    if (schedule == null)
      throw new IllegalArgumentException("Parameter 'schedule' is required.");

    if (schedule.getRepeat() > 0)
      dataSource.setRanCounter(dataSource.getRanCounter() + 1);
    dataSource.setLastRanOn(ranOn);
  }
}
