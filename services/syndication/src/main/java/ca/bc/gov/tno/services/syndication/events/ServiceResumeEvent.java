package ca.bc.gov.tno.services.syndication.events;

import org.springframework.context.ApplicationEvent;

/**
 * ConsumerResumeEvent class, provides an event to inform Kafka consumer to
 * resume.
 */
public class ServiceResumeEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an ConsumerResumeEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public ServiceResumeEvent(Object source) {
    super(source);
  }
}
