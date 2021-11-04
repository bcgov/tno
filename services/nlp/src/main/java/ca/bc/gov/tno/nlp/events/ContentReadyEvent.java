package ca.bc.gov.tno.nlp.events;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.nlp.models.NlpContent;

/**
 * ContentReadyEvent class, provides an event to indicate content has been
 * processed by NLP and is ready to be returned to Kafka.
 */
@Async
public class ContentReadyEvent extends ApplicationEvent {
  private NlpContent content;

  /**
   * Creates a new instance of an ContentReadyEvent, initializes with specified
   * parameters.
   * 
   * @param source
   * @param content
   */
  public ContentReadyEvent(Object source, NlpContent content) {
    super(source);
    this.content = content;
  }

  public NlpContent getContent() {
    return content;
  }
}
