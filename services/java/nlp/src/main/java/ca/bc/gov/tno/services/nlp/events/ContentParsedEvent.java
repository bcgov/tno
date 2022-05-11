package ca.bc.gov.tno.services.nlp.events;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.models.NlpContent;
import ca.bc.gov.tno.models.SourceContent;

/**
 * ContentParsedEvent class, provides an event when content has been parsed and
 * is now ready for NLP processing.
 */
@Async
public class ContentParsedEvent extends ApplicationEvent {
  private SourceContent content;
  private NlpContent result;

  /**
   * Creates a new instance of an ContentParsedEvent, initializes with specified
   * parameters.
   * 
   * @param source
   * @param content
   * @param result
   */
  public ContentParsedEvent(Object source, SourceContent content, NlpContent result) {
    super(source);
    this.content = content;
    this.result = result;
  }

  public SourceContent getContent() {
    return content;
  }

  public NlpContent getResult() {
    return result;
  }
}
