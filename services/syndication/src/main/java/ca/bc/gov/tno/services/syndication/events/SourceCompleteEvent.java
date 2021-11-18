package ca.bc.gov.tno.services.syndication.events;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.syndication.config.SyndicationConfig;

/**
 * SourceCompleteEvent class, provides an event to indicate the source has
 * completed processing.
 */
@Async
public class SourceCompleteEvent extends ApplicationEvent {

  private SyndicationConfig config;

  /**
   * Creates a new instance of an SourceCompleteEvent, initializes with specified
   * parameters.
   * 
   * @param source
   * @param config
   */
  public SourceCompleteEvent(Object source, SyndicationConfig config) {
    super(source);
    this.config = config;
  }

  /**
   * Get the config;
   */
  public SyndicationConfig getConfig() {
    return config;
  }
}
