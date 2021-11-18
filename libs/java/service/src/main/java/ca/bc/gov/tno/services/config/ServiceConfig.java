package ca.bc.gov.tno.services.config;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;

/**
 * ServiceConfig class, provides configuration settings for Application
 * Services.
 */
@Configuration
public class ServiceConfig {
  /**
   * Maximum number of attempts after a failure.
   */
  @Value("${service.max-failed-attempts}")
  private int maxFailedAttempts;

  /**
   * Number of milliseconds before making another attempt after a failure.
   */
  @Value("${service.failed-delay}")
  private int failedDelay;

  /**
   * @return int return the maxFailedAttempts
   */
  public int getMaxFailedAttempts() {
    return maxFailedAttempts;
  }

  /**
   * @param maxFailedAttempts the maxFailedAttempts to set
   */
  public void setMaxFailedAttempts(int maxFailedAttempts) {
    this.maxFailedAttempts = maxFailedAttempts;
  }

  /**
   * @return int return the failedDelay
   */
  public int getFailedDelay() {
    return failedDelay;
  }

  /**
   * @param failedDelay the failedDelay to set
   */
  public void setFailedDelay(int failedDelay) {
    this.failedDelay = failedDelay;
  }

}
