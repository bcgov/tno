package ca.bc.gov.tno.services.nlp.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerResumeEvent class, provides an event to inform Kafka consumer to
 * resume.
 */
public class ConsumerResumeEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerResumeEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ConsumerResumeEvent(Object source) {
    super(source);
  }
}
