package ca.bc.gov.tno.services.kafka.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationListener;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.kafka.events.ConsumerResumeEvent;

/**
 * ResumeHandler class, provides a listener for the ConsumerResumeEvent. It will
 * change the service state to resume.
 */
@Async
@Service
public class ConsumerResumeHandler implements ApplicationListener<ConsumerResumeEvent> {
  private static final Logger logger = LogManager.getLogger(ConsumerPauseHandler.class);

  private final ServiceState state;

  /**
   * Create a new instance of a ResumeHandler object, initializes with specified
   * parameters.
   * 
   * @param state Service state.
   */
  @Autowired
  public ConsumerResumeHandler(final ServiceState state) {
    this.state = state;
  }

  /**
   * Change the state of the service to resume.
   * 
   * @param event The event.
   */
  @Override
  public void onApplicationEvent(ConsumerResumeEvent event) {
    logger.info("Resume consumer requested");
    state.setStatus(ServiceStatus.resume);
  }
}
