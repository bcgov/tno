package ca.bc.gov.tno.services.events;

import org.springframework.context.ApplicationEvent;

/**
 * ServicePauseEvent class, provides an event to inform the service to pause
 */
public class ServicePauseEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ServicePauseEvent, initializes with specified
   * parameters.
   * 
   * @param source The source of the event.
   */
  public ServicePauseEvent(final Object source) {
    super(source);
  }
}
