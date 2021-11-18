package ca.bc.gov.tno.services.syndication;

import org.springframework.stereotype.Component;

import ca.bc.gov.tno.services.ServiceStatus;

/**
 * Global class, provides a way tto manage state within the service.
 */
@Component
public class Global {
  /**
   * The current status of the service.
   */
  private ServiceStatus status = ServiceStatus.running;

  /**
   * Number of failed attempts.
   */
  private int failedAttempts;

  /**
   * @return int return the failedAttempts
   */
  public int getFailedAttempts() {
    return failedAttempts;
  }

  /**
   * @param failedAttempts the failedAttempts to set
   */
  public void setFailedAttempts(int failedAttempts) {
    this.failedAttempts = failedAttempts;
  }

  public void incrementFailedAttempts() {
    this.failedAttempts++;
  }

  /**
   * @return ServiceStatus return the status
   */
  public ServiceStatus getStatus() {
    return status;
  }

  /**
   * @param status the status to set
   */
  public void setStatus(ServiceStatus status) {
    this.status = status;
  }

}
