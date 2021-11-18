package ca.bc.gov.tno.services.nlp.events;

import org.springframework.context.ApplicationEvent;

/**
 * ServiceStopEvent class, provides an event to request stopping the service.
 */
public class ServiceStopEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ServiceStopEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ServiceStopEvent(Object source) {
    super(source);
  }
}
