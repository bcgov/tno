package ca.bc.gov.tno.services.syndication.events;

import com.rometools.rome.feed.synd.SyndFeed;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.syndication.config.SyndicationConfig;

/**
 * SendToKafkaEvent class, provides an event to indicate the syndication feed
 * has been loaded.
 */
@Async
public class ProducerSendEvent extends ApplicationEvent {
  /**
   * The data source config.
   */
  private final SyndicationConfig config;

  /**
   * The data to send to Kafka.
   */
  private final SyndFeed data;

  /**
   * Creates a new instance of an SendToKafkaEvent, initializes with specified
   * parameters.
   * 
   * @param source
   * @param config
   * @param data
   */
  public ProducerSendEvent(final Object source, final SyndicationConfig config, final SyndFeed data) {
    super(source);
    this.config = config;
    this.data = data;
  }

  /**
   * Get the data source configuration.
   * 
   * @return The data source config.
   */
  public SyndicationConfig getConfig() {
    return config;
  }

  /**
   * Get the data that will be sent to Kafka.
   * 
   * @return The data to send to Kafka.
   */
  public SyndFeed getData() {
    return data;
  }
}
