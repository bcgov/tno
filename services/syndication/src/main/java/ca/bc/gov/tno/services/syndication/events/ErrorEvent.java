package ca.bc.gov.tno.services.syndication.events;

import org.springframework.context.ApplicationEvent;

/**
 * ErrorEvent class, provides an event that indicates an error has occurred.
 */
public class ErrorEvent extends ApplicationEvent {
  private Exception exception;

  /**
   * Creates a new instance of an ErrorEvent, initializes with specified
   * parameters.
   * 
   * @param source
   * @param exception
   */
  public ErrorEvent(Object source, Exception exception) {
    super(source);
    this.exception = exception;
  }

  public Exception getError() {
    return exception;
  }
}
