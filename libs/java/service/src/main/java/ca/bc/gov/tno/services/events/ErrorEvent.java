package ca.bc.gov.tno.services.events;

import org.springframework.context.ApplicationEvent;

import ca.bc.gov.tno.services.data.config.DataSourceConfig;

/**
 * ErrorEvent class, provides an event that indicates an error has occurred.
 */
public class ErrorEvent extends ApplicationEvent {
  /**
   * The exception that was thrown.
   */
  private final Exception exception;

  /**
   * The data source being processed.
   */
  private final DataSourceConfig config;

  /**
   * Creates a new instance of an ErrorEvent, initializes with specified
   * parameters. Retained while testing ongoing in select services.
   * 
   * @param source    The source of the event.
   * @param exception The exception that was thrown.
   */
  public ErrorEvent(final Object source, final Exception exception) {
    super(source);
    this.exception = exception;
    this.config = null;
  }

  /**
   * Creates a new instance of an ErrorEvent, initializes with specified
   * parameters.
   * 
   * @param source    The source of the event.
   * @param config    The data-source config.
   * @param exception The exception that was thrown.
   */
  public ErrorEvent(final Object source, final DataSourceConfig config, final Exception exception) {
    super(source);
    this.config = config;
    this.exception = exception;
  }

  /**
   * Get the exception that was thrown.
   * 
   * @return The exception that was thrown.
   */
  public Exception getError() {
    return exception;
  }

  /**
   * Get the config for the data data source being processed.
   * 
   * @return The exception that was thrown.
   */
  public DataSourceConfig getConfig() {
    return config;
  }
}
