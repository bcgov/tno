package ca.bc.gov.tno.services.audio.events;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.audio.config.AudioConfig;

/**
 * SendToKafkaEvent class, provides an event to indicate the audio feed
 * has been loaded.
 */
@Async
public class ProducerSendEvent extends ApplicationEvent {
  /**
   * The data source config.
   */
  private final AudioConfig config;

  /**
   * The data to send to Kafka.
   */
  private final String data;

  /**
   * Creates a new instance of an SendToKafkaEvent, initializes with specified
   * parameters.
   * 
   * @param source
   * @param config
   * @param data
   */
  public ProducerSendEvent(final Object source, final AudioConfig config, String data) {
    super(source);
    this.config = config;
    this.data = data;
  }

  /**
   * Get the data source configuration.
   * 
   * @return The data source config.
   */
  public AudioConfig getConfig() {
    return config;
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
