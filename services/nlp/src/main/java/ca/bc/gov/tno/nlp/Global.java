package ca.bc.gov.tno.nlp;

import org.springframework.stereotype.Component;

@Component
public class Global {
  private NlpStatus status = NlpStatus.running;

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
  public NlpStatus getStatus() {
    return status;
  }

  /**
   * @param status the status to set
   */
  public void setStatus(NlpStatus status) {
    this.status = status;
  }

}
