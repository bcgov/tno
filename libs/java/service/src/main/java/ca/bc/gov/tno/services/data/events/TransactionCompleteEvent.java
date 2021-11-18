package ca.bc.gov.tno.services.data.events;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.data.config.DataSourceConfig;

/**
 * TransactionCompleteEvent class, provides an event to indicate the source has
 * completed processing.
 * 
 * @param <T> The data source config type.
 */
@Async
public class TransactionCompleteEvent<T extends DataSourceConfig> extends ApplicationEvent {

  /**
   * The data source configuration.
   */
  private T config;

  /**
   * Creates a new instance of an TransactionCompleteEvent, initializes with
   * specified parameters.
   * 
   * @param source The source of the event.
   * @param config The data source config.
   */
  public TransactionCompleteEvent(Object source, T config) {
    super(source);
    this.config = config;
  }

  /**
   * Get the config;
   * 
   * @return The data source config.
   */
  public T getConfig() {
    return config;
  }
}
