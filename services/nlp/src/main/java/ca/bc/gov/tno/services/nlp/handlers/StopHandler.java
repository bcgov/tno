package ca.bc.gov.tno.services.nlp.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationListener;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.nlp.Global;
import ca.bc.gov.tno.services.nlp.events.ServiceStopEvent;

/**
 * StopHandler class, provides a way to handle errors and increment the failed
 * attempts. Once the failed attempts is equal to the maximum limit it will
 * inform the application to stop.
 */
@Component
public class StopHandler implements ApplicationListener<ServiceStopEvent> {
  private static final Logger logger = LogManager.getLogger(StopHandler.class);

  @Autowired
  private Global global;

  /**
   * Log the error. Update the current failed attempts. Inform the application if
   * it should stop.
   */
  @Override
  public void onApplicationEvent(ServiceStopEvent event) {
    logger.info("Service stop requested");
    global.setStatus(ServiceStatus.sleeping);
  }
}
