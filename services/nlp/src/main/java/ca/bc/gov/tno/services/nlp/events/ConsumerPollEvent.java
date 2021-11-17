package ca.bc.gov.tno.services.nlp.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerPollEvent class, provides an event when the Kafka consumer has
 * successfully polled a topic.
 */
public class ConsumerPollEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerPollEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ConsumerPollEvent(Object source) {
    super(source);
  }
}
