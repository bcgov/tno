package ca.bc.gov.tno.services;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.services.config.ServiceConfig;
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
      final ServiceConfig config) {
    this.eventPublisher = eventPublisher;
    this.state = state;
    this.config = config;
  }

  /**
   * Log the error. Update the current failed attempts. Inform the application if
   * it should stop.
   */
  @Override
  public void onApplicationEvent(ErrorEvent event) {
    var source = event.getSource();
    var exception = event.getError();
    logger.error(String.format("Source: '%s'", source.getClass().getName()), exception);

    if (state.getFailedAttempts() >= config.getMaxFailedAttempts()) {
      state.setStatus(ServiceStatus.sleeping);
    } else {
      state.incrementFailedAttempts();
      if (state.getStatus() == ServiceStatus.sleeping) {
        var startEvent = new ServiceStartEvent(this);
        eventPublisher.publishEvent(startEvent);
      }
    }
  }
}
