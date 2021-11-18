package ca.bc.gov.tno.services.syndication.controllers;

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
import ca.bc.gov.tno.services.syndication.Global;
import ca.bc.gov.tno.services.syndication.events.ServicePauseEvent;
import ca.bc.gov.tno.services.syndication.events.ServiceResumeEvent;
import ca.bc.gov.tno.services.syndication.events.ServiceStartEvent;
import ca.bc.gov.tno.services.syndication.events.ServiceStopEvent;

/**
 * HealthController class, provides an REST API controller for health endpoints.
 */
@RestController
public class HealthController {
  @Autowired
  private Global global;

  @Autowired
  private ApplicationEventPublisher applicationEventPublisher;

  /**
   * Ask for the current health of the service.
   * 
   * @return
   */
  @GetMapping(value = "/health", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> health() {
    return new ResponseEntity<>(new HealthReport(global.getStatus(), global.getFailedAttempts()), HttpStatus.OK);
  }

  /**
   * Request the service to start processing requests.
   * 
   * @return
   */
  @PutMapping(value = "/start", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> start() {
    if (global.getStatus() == ServiceStatus.sleeping) {
      var event = new ServiceStartEvent(this);
      applicationEventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(global.getStatus(), global.getFailedAttempts()), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(global.getStatus(), global.getFailedAttempts()),
        HttpStatus.BAD_REQUEST);
  }

  /**
   * Request the service to stop processing requests.
   * 
   * @return
   */
  @PutMapping(value = "/stop", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> stop() {
    var status = global.getStatus();
    if (status == ServiceStatus.running || status == ServiceStatus.paused) {
      var event = new ServiceStopEvent(this);
      applicationEventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(global.getStatus(), global.getFailedAttempts()), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(global.getStatus(), global.getFailedAttempts()),
        HttpStatus.BAD_REQUEST);
  }

  /**
   * Request the service to pause.
   * 
   * @return
   */
  @PutMapping(value = "/pause", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> pause() {
    if (global.getStatus() == ServiceStatus.running) {
      var event = new ServicePauseEvent(this);
      applicationEventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(global.getStatus(), global.getFailedAttempts()), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(global.getStatus(), global.getFailedAttempts()),
        HttpStatus.BAD_REQUEST);
  }

  /**
   * Request the service to resume.
   * 
   * @return
   */
  @PutMapping(value = "/resume", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> resume() {
    if (global.getStatus() == ServiceStatus.paused) {
      var event = new ServiceResumeEvent(this);
      applicationEventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(global.getStatus(), global.getFailedAttempts()), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(global.getStatus(), global.getFailedAttempts()),
        HttpStatus.BAD_REQUEST);
  }
}
