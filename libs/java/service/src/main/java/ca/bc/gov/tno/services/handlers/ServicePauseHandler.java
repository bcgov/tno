package ca.bc.gov.tno.services.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationListener;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.events.ServicePauseEvent;

/**
 * PauseHandler class, provides a listener for the ServicePauseEvent. It will
 * change the service state to pause.
 */
@Async
@Service
public class ServicePauseHandler implements ApplicationListener<ServicePauseEvent> {
  private static final Logger logger = LogManager.getLogger(ServicePauseHandler.class);

  private final ServiceState state;

  /**
   * Create a new instance of a PauseHandler object, initializes with specified
   * parameters.
   * 
   * @param state Service state.
   */
  @Autowired
  public ServicePauseHandler(final ServiceState state) {
    this.state = state;
  }

  /**
   * Change the state of the service to pause.
   * 
   * @param event The event.
   */
  @Override
  public void onApplicationEvent(ServicePauseEvent event) {
    logger.info("Pause Service requested");
    state.setStatus(ServiceStatus.pause);
  }
}
