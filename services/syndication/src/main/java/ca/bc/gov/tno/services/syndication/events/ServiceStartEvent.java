package ca.bc.gov.tno.services.syndication.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerStartEvent class, provides an event to request starting the kafka
 * consumer.
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
