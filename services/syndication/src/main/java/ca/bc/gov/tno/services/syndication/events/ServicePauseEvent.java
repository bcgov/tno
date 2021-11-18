package ca.bc.gov.tno.services.syndication.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerPauseEvent class, provides an event to inform Kafka consumer to pause
 */
public class ServicePauseEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerPauseEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ServicePauseEvent(Object source) {
    super(source);
  }
}
