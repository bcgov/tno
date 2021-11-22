package ca.bc.gov.tno.services.data.events;

import org.springframework.context.ApplicationEvent;
import org.springframework.scheduling.annotation.Async;

import ca.bc.gov.tno.services.data.config.DataSourceConfig;

/**
 * TransactionBeginEvent class, provides an event to indicate the transaction
 * has begun.
 */
@Async
public class TransactionBeginEvent<T extends DataSourceConfig> extends ApplicationEvent {

  /**
   * The data source configuration.
   */
  private T config;

  /**
   * Creates a new instance of an TransactionBeginEvent, initializes with
   * specified parameters.
   * 
   * @param source The source of the event.
   * @param config Data source configuration
   */
  public TransactionBeginEvent(Object source, T config) {
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
