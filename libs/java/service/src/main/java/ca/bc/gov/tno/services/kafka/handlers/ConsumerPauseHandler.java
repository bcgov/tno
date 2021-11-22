package ca.bc.gov.tno.services.kafka.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationListener;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.kafka.events.ConsumerPauseEvent;

/**
 * PauseHandler class, provides a listener for the ConsumerPauseEvent. It will
 * change the service state to pause.
 */
@Async
@Service
public class ConsumerPauseHandler implements ApplicationListener<ConsumerPauseEvent> {
  private static final Logger logger = LogManager.getLogger(ConsumerPauseHandler.class);

  private final ServiceState state;

  /**
   * Create a new instance of a PauseHandler object, initializes with specified
   * parameters.
   * 
   * @param state Service state.
   */
  @Autowired
  public ConsumerPauseHandler(final ServiceState state) {
    this.state = state;
  }

  /**
   * Change the state of the service to pause.
   * 
   * @param event The event.
   */
  @Override
  public void onApplicationEvent(ConsumerPauseEvent event) {
    logger.info("Pause consumer requested");
    state.setStatus(ServiceStatus.pause);
  }
}
