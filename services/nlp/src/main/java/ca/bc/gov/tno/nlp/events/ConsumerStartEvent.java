package ca.bc.gov.tno.nlp.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerStartEvent class, provides an event to request starting the kafka
 * consumer.
 */
public class ConsumerStartEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerStartEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ConsumerStartEvent(Object source) {
    super(source);
  }
}
