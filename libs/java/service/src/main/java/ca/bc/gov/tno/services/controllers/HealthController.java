package ca.bc.gov.tno.services.controllers;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.services.models.HealthReport;
import ca.bc.gov.tno.services.ServiceStatus;
import ca.bc.gov.tno.services.ServiceState;
import ca.bc.gov.tno.services.kafka.events.ConsumerPauseEvent;
import ca.bc.gov.tno.services.kafka.events.ConsumerResumeEvent;
import ca.bc.gov.tno.services.events.ServiceStartEvent;
import ca.bc.gov.tno.services.events.ServiceStopEvent;

/**
 * HealthController class, provides an REST API controller for health endpoints.
 */
@RestController
public class HealthController {
  private final ServiceState state;

  private final ApplicationEventPublisher eventPublisher;

  /**
   * Creates a new instance of a HealthController object, initializes with
   * specified parameters.
   * 
   * @param state          Service state
   * @param eventPublisher Application event publisher.
   */
  @Autowired
  public HealthController(final ServiceState state, final ApplicationEventPublisher eventPublisher) {
    this.state = state;
    this.eventPublisher = eventPublisher;
  }

  /**
   * Ask for the current health of the service.
   * 
   * @return The response.
   */
  @GetMapping(value = "/health", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> health() {
    return new ResponseEntity<>(new HealthReport(state.getStatus(), state.getFailedAttempts()), HttpStatus.OK);
  }

  /**
   * Request the NLP service to start processing requests.
   * 
   * @return The response.
   */
  @PutMapping(value = "/start", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> start() {
    if (state.getStatus() == ServiceStatus.sleeping) {
      var event = new ServiceStartEvent(this);
      eventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(state.getStatus(), state.getFailedAttempts()), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(state.getStatus(), state.getFailedAttempts()), HttpStatus.BAD_REQUEST);
  }

  /**
   * Request the NLP service to stop processing requests.
   * 
   * @return The response.
   */
  @PutMapping(value = "/stop", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> stop() {
    var status = state.getStatus();
    if (status == ServiceStatus.running || status == ServiceStatus.paused) {
      var event = new ServiceStopEvent(this);
      eventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(state.getStatus(), state.getFailedAttempts()), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(state.getStatus(), state.getFailedAttempts()), HttpStatus.BAD_REQUEST);
  }

  /**
   * Request the kafka consumer to pause.
   * 
   * @return The response.
   */
  @PutMapping(value = "/pause", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> pause() {
    if (state.getStatus() == ServiceStatus.running) {
      var event = new ConsumerPauseEvent(this);
      eventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(state.getStatus(), state.getFailedAttempts()), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(state.getStatus(), state.getFailedAttempts()), HttpStatus.BAD_REQUEST);
  }

  /**
   * Request the kafka consumer to resume.
   * 
   * @return The response.
   */
  @PutMapping(value = "/resume", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> resume() {
    if (state.getStatus() == ServiceStatus.paused) {
      var event = new ConsumerResumeEvent(this);
      eventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(state.getStatus(), state.getFailedAttempts()), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(state.getStatus(), state.getFailedAttempts()), HttpStatus.BAD_REQUEST);
  }
}
