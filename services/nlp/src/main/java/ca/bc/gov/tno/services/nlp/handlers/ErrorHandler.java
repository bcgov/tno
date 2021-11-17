package ca.bc.gov.tno.services.nlp.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.context.ApplicationListener;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.nlp.Global;
import ca.bc.gov.tno.services.nlp.config.ServiceConfig;
import ca.bc.gov.tno.services.nlp.events.ErrorEvent;
import ca.bc.gov.tno.services.nlp.events.ServiceStartEvent;

/**
 * ErrorHandler class, provides a way to handle errors and increment the failed
 * attempts. Once the failed attempts is equal to the maximum limit it will
 * inform the application to stop.
 */
@Component
public class ErrorHandler implements ApplicationListener<ErrorEvent> {
  private static final Logger logger = LogManager.getLogger(ErrorHandler.class);

  @Autowired
  private ApplicationEventPublisher applicationEventPublisher;

  @Autowired
  private Global global;

  @Autowired
  private ServiceConfig config;

  /**
   * Log the error. Update the current failed attempts. Inform the application if
   * it should stop.
   */
  @Override
  public void onApplicationEvent(ErrorEvent event) {
    var source = event.getSource();
    var exception = event.getError();
    logger.error(String.format("Source: '%s'", source.getClass().getName()), exception);

    if (global.getFailedAttempts() >= config.getMaxFailedAttempts()) {
      global.setStatus(ServiceStatus.sleeping);
    } else {
      global.incrementFailedAttempts();
      if (global.getStatus() == ServiceStatus.sleeping) {
        var startEvent = new ServiceStartEvent(this);
        applicationEventPublisher.publishEvent(startEvent);
      }
    }
  }
}
