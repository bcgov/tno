package ca.bc.gov.tno.services.syndication.events;

import com.rometools.rome.feed.synd.SyndFeed;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.data.config.ScheduleConfig;
import ca.bc.gov.tno.services.syndication.config.SyndicationConfig;

/**
 * ProducerSendEvent class, provides an event to indicate the syndication feed
 * has been loaded.
 */
@Async
public class ProducerSendEvent extends ApplicationEvent {
  /**
   * The data source configuration.
   */
  private final SyndicationConfig dataSource;

  /**
   * The schedule configuration.
   */
  private final ScheduleConfig schedule;

  /**
   * The data to send to Kafka.
   */
  private final SyndFeed data;

  /**
   * Creates a new instance of an SendToKafkaEvent, initializes with specified
   * parameters.
   * 
   * @param source     The source of this event.
   * @param dataSource The data source config.
   * @param schedule   The schedule config.
   * @param data       The syndication feed.
   */
  public ProducerSendEvent(final Object source, final SyndicationConfig dataSource, final ScheduleConfig schedule,
      final SyndFeed data) {
    super(source);
    this.dataSource = dataSource;
    this.schedule = schedule;
    this.data = data;
  }

  /**
   * Get the data source configuration.
   * 
   * @return The data source config.
   */
  public SyndicationConfig getDataSource() {
    return dataSource;
  }

  /**
   * Get the schedule configuration.
   * 
   * @return The schedule config.
   */
  public ScheduleConfig getSchedule() {
    return schedule;
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
