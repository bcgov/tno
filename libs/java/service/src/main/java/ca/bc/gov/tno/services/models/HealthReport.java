package ca.bc.gov.tno.services.models;

import ca.bc.gov.tno.services.ServiceStatus;

/**
 * HealthReport class, provides a model that represents service status.
 */
public class HealthReport {
  /**
   * The status of the service.
   */
  public ServiceStatus status;

  /**
   * The number of failed attempts.
   */
  public int failedAttempts;

  /**
   * Creates a new instance of a HealthReport object.
   */
  public HealthReport() {

  }

  /**
   * Creates a new instance of a HealthReport object, initializes with specified
   * parameters.
   * 
   * @param status         The status of the service.
   * @param failedAttempts The number of failed attempts.
   */
  public HealthReport(final ServiceStatus status, final int failedAttempts) {
    this.status = status;
    this.failedAttempts = failedAttempts;
  }
}
