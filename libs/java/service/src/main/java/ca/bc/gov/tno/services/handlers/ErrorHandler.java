package ca.bc.gov.tno.services.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.dal.db.services.DataSourceService;
import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.config.ServiceConfig;
import ca.bc.gov.tno.services.data.BaseScheduleService;
import ca.bc.gov.tno.services.events.ErrorEvent;
import ca.bc.gov.tno.services.events.ServiceStartEvent;

/**
 * ErrorHandler class, provides a way to handle errors and increment the failed
 * attempts. Once the failed attempts is equal to the maximum limit it will
 * inform the application to stop.
 */
@Service
public class ErrorHandler implements ApplicationListener<ErrorEvent> {
  private static final Logger logger = LogManager.getLogger(ErrorHandler.class);

  private final ApplicationEventPublisher eventPublisher;

  private final ServiceState state;

  private final ServiceConfig config;

  private final DataSourceService dataSourceService;

  /**
   * Creates a new instance of an ErrorHandler object, initializes with specified
   * parameters.
   * 
   * @param state          Service state.
   * @param eventPublisher Application event publisher.
   * @param config         Service configuration.
   */
  @Autowired
  public ErrorHandler(final ServiceState state, final ApplicationEventPublisher eventPublisher,
      final ServiceConfig config, final DataSourceService dsService) {
    this.eventPublisher = eventPublisher;
    this.state = state;
    this.config = config;
    this.dataSourceService = dsService;
  }

  /**
   * Log the error. Update the current failed attempts. Inform the application if
   * it should stop.
   */
  @Override
  public void onApplicationEvent(ErrorEvent event) {
    var source = event.getSource();
    var exception = event.getError();
    var dsConfig = event.getConfig();
    logger.error(String.format("Source: '%s'", source.getClass().getName()), exception);

    if (state.getFailedAttempts() >= config.getMaxFailedAttempts()) {
      state.setStatus(ServiceStatus.sleeping);
    } else {
      state.incrementFailedAttempts();
      
      if (dsConfig != null) {
        dsConfig.failedAttempts++;
        var result = dataSourceService.findByCode(dsConfig.getId());
        var dataSource = result.get();
        dataSource.setFailedAttempts(dsConfig.getFailedAttempts());
        dataSourceService.update(dataSource);
      }

      if (state.getStatus() == ServiceStatus.sleeping) {
        var startEvent = new ServiceStartEvent(this);
        eventPublisher.publishEvent(startEvent);
      }
    }

    if (source instanceof BaseScheduleService) {
      synchronized(source) {
        source.notify();
      }
    } 
  }
}
