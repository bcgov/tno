package ca.bc.gov.tno.services.handlers;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationListener;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.events.ServiceResumeEvent;

/**
 * ResumeHandler class, provides a listener for the ServiceResumeEvent. It will
 * change the service state to resume.
 */
@Async
@Service
public class ServiceResumeHandler implements ApplicationListener<ServiceResumeEvent> {
  private static final Logger logger = LogManager.getLogger(ServicePauseHandler.class);

  private final ServiceState state;

  /**
   * Create a new instance of a ResumeHandler object, initializes with specified
   * parameters.
   * 
   * @param state Service state.
   */
  @Autowired
  public ServiceResumeHandler(final ServiceState state) {
    this.state = state;
  }

  /**
   * Change the state of the service to resume.
   * 
   * @param event The event.
   */
  @Override
  public void onApplicationEvent(ServiceResumeEvent event) {
    logger.info("Resume Service requested");
    state.setStatus(ServiceStatus.resume);
  }
}
