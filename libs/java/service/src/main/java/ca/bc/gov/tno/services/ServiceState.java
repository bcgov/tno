package ca.bc.gov.tno.services;

import org.springframework.stereotype.Component;

/**
 * ServiceState class, provides a way to manage state for this service.
 */
@Component
public class ServiceState {
  /**
   * The service status.
   */
  private ServiceStatus status = ServiceStatus.running;

  /**
   * The number of failed attempts.
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

  /**
   * Increment the failed attempts.
   */
  public void incrementFailedAttempts() {
    this.failedAttempts++;
  }

  /**
   * @return NlpStatus return the status
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
