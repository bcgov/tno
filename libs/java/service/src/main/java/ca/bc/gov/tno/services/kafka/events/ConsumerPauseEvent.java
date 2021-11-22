package ca.bc.gov.tno.services.kafka.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerPauseEvent class, provides an event to inform Kafka consumer to pause
 */
public class ConsumerPauseEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerPauseEvent, initializes with specified
   * parameters.
   * 
   * @param source The source of the event.
   */
  public ConsumerPauseEvent(final Object source) {
    super(source);
  }
}
