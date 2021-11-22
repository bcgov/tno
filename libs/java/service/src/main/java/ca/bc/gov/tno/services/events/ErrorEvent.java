package ca.bc.gov.tno.services.events;

import org.springframework.context.ApplicationEvent;

/**
 * ErrorEvent class, provides an event that indicates an error has occurred.
 */
public class ErrorEvent extends ApplicationEvent {
  /**
   * The exception that was thrown.
   */
  private final Exception exception;

  /**
   * Creates a new instance of an ErrorEvent, initializes with specified
   * parameters.
   * 
   * @param source    The source of the event.
   * @param exception The exception that was thrown.
   */
  public ErrorEvent(final Object source, final Exception exception) {
    super(source);
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
}
