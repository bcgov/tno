package ca.bc.gov.tno.services.syndication.events;

import org.springframework.context.ApplicationEvent;

import ca.bc.gov.tno.services.syndication.config.SyndicationConfig;

/**
 * FetchEvent class, provides an event when the a fetch has been issued.
 */
public class FetchEvent extends ApplicationEvent {

  /**
   * Syndication configuration settings.
   */
  private SyndicationConfig config;

  /**
   * Creates a new instance of an FetchEvent, initializes with specified
   * parameters.
   * 
   * @param source
   * @param config
   */
  public FetchEvent(Object source, SyndicationConfig config) {
    super(source);

    this.config = config;
  }

  public SyndicationConfig getConfig() {
    return config;
  }
}
