package ca.bc.gov.tno.services.syndication.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerStopEvent class, provides an event when the Kafka consumer has been
 * stopped.
 */
public class ServiceStopEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerStopEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ServiceStopEvent(Object source) {
    super(source);
  }
}
