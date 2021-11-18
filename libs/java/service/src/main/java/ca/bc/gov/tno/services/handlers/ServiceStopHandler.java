package ca.bc.gov.tno.services.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationListener;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.events.ServiceStopEvent;

/**
 * StopHandler class, provides a listener for the ServiceStopEvent. It will
 * change the service state to sleeping.
 */
@Async
@Service
public class ServiceStopHandler implements ApplicationListener<ServiceStopEvent> {
  private static final Logger logger = LogManager.getLogger(ServiceStopHandler.class);

  private final ServiceState state;

  /**
   * Create a new instance of a StopHandler object, initializes with specified
   * parameters.
   * 
   * @param state Service state.
   */
  @Autowired
  public ServiceStopHandler(final ServiceState state) {
    this.state = state;
  }

  /**
   * Change the state of the service to sleeping.
   * 
   * @param event The event.
   */
  @Override
  public void onApplicationEvent(ServiceStopEvent event) {
    logger.info("Stop service requested");
    state.setStatus(ServiceStatus.sleeping);
  }
}
