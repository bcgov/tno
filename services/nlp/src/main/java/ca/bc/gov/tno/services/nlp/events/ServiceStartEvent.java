package ca.bc.gov.tno.services.nlp.events;

import org.springframework.context.ApplicationEvent;

/**
 * ServiceStartEvent class, provides an event to request starting the service.
 */
public class ServiceStartEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerStartEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ServiceStartEvent(Object source) {
    super(source);
  }
}
