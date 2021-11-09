package ca.bc.gov.tno.nlp.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerStopEvent class, provides an event when the Kafka consumer has been
 * stopped.
 */
public class ConsumerStopEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerStopEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ConsumerStopEvent(Object source) {
    super(source);
  }
}
