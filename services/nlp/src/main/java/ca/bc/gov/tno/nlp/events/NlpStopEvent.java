package ca.bc.gov.tno.nlp.events;

import org.springframework.context.ApplicationEvent;

/**
 * NlpStopEvent class, provides an event to request stopping the NLP service.
 */
public class NlpStopEvent extends ApplicationEvent {
  /**
   * Creates a new instance of an NlpStopEvent, initializes with specified
   * parameters.
   * 
   * @param source
   */
  public NlpStopEvent(Object source) {
    super(source);
  }
}
