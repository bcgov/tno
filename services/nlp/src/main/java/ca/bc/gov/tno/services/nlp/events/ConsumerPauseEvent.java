package ca.bc.gov.tno.services.nlp.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerPauseEvent class, provides an event to inform Kafka consumer to pause
 */
public class ConsumerPauseEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerPauseEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ConsumerPauseEvent(Object source) {
    super(source);
  }
}
