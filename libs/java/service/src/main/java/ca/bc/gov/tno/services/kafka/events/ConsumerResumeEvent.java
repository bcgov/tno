package ca.bc.gov.tno.services.kafka.events;

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
   * @param source The source of the event.
   */
  public ConsumerResumeEvent(final Object source) {
    super(source);
  }
}
