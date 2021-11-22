package ca.bc.gov.tno.services.events;

import org.springframework.context.ApplicationEvent;

/**
 * ServiceResumeEvent class, provides an event to inform the service to resume.
 */
public class ServiceResumeEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ServiceResumeEvent, initializes with specified
   * parameters.
   * 
   * @param source The source of the event.
   */
  public ServiceResumeEvent(final Object source) {
    super(source);
  }
}
