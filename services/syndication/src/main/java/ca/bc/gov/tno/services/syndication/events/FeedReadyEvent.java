package ca.bc.gov.tno.services.syndication.events;

import com.rometools.rome.feed.synd.SyndFeed;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.syndication.config.SyndicationConfig;

/**
 * FeedReadyEvent class, provides an event to indicate the syndication feed has
 * been loaded.
 */
@Async
public class FeedReadyEvent extends ApplicationEvent {
  private SyndFeed feed;
  private SyndicationConfig config;

  /**
   * Creates a new instance of an FeedReadyEvent, initializes with specified
   * parameters.
   * 
   * @param source
   * @param config
   * @param feed
   */
  public FeedReadyEvent(Object source, SyndicationConfig config, SyndFeed feed) {
    super(source);
    this.config = config;
    this.feed = feed;
  }

  public SyndicationConfig getConfig() {
    return config;
  }

  public SyndFeed getFeed() {
    return feed;
  }
}
