package ca.bc.gov.tno.services.nlp;

import org.springframework.stereotype.Component;

import ca.bc.gov.tno.services.ServiceStatus;

@Component
public class Global {
  private ServiceStatus status = ServiceStatus.running;

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
