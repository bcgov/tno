package ca.bc.gov.tno.services.capture.events;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.capture.config.CaptureConfig;
import ca.bc.gov.tno.services.data.config.ScheduleConfig;

/**
 * SendToKafkaEvent class, provides an event to indicate the capture process
 * has been started.
 */
@Async
public class ProducerSendEvent extends ApplicationEvent {
  /**
   * The data source configuration.
   */
  private final CaptureConfig dataSource;

  /**
   * The schedule configuration.
   */
  private final ScheduleConfig schedule;

  /**
   * The data to store in Kafka
   */
  private final String data;

  /**
   * Creates a new instance of an SendToKafkaEvent, initializes with specified
   * parameters.
   * 
   * @param source     The source of this event.
   * @param dataSource The data source config.
   * @param schedule   The schedule config.
   * @param data       The syndication feed.
   */
  public ProducerSendEvent(final Object source, final CaptureConfig dataSource, final ScheduleConfig schedule, final String data) {
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
  public CaptureConfig getDataSource() {
    return dataSource;
  }

  /**
   * Get the data that will be sent to Kafka.
   * 
   * @return The data to send to Kafka.
   */
  public ScheduleConfig getSchedule() {
    return schedule;
  } 

  /**
   * Get the data that will be sent to Kafka.
   * 
   * @return The data to send to Kafka.
   */
  public String getData() {
    return data;
  } 
}
