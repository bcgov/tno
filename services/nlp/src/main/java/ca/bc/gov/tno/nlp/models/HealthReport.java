package ca.bc.gov.tno.nlp.models;

import ca.bc.gov.tno.nlp.Global;
import ca.bc.gov.tno.nlp.NlpStatus;

public class HealthReport {
  public NlpStatus status;

  public int failedAttempts;

  public HealthReport() {

  }

  public HealthReport(Global global) {
    this.status = global.getStatus();
    this.failedAttempts = global.getFailedAttempts();
  }
}
