package ca.bc.gov.tno.nlp.controllers;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.nlp.Global;
import ca.bc.gov.tno.nlp.NlpStatus;
import ca.bc.gov.tno.nlp.events.ConsumerPauseEvent;
import ca.bc.gov.tno.nlp.events.ConsumerResumeEvent;
import ca.bc.gov.tno.nlp.events.ConsumerStartEvent;
import ca.bc.gov.tno.nlp.events.NlpStopEvent;
import ca.bc.gov.tno.nlp.models.HealthReport;

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
    return new ResponseEntity<>(new HealthReport(global), HttpStatus.OK);
  }

  /**
   * Request the NLP service to start processing requests.
   * 
   * @return
   */
  @PutMapping(value = "/start", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> start() {
    if (global.getStatus() == NlpStatus.sleeping) {
      var event = new ConsumerStartEvent(this);
      applicationEventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(global), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(global), HttpStatus.BAD_REQUEST);
  }

  /**
   * Request the NLP service to stop processing requests.
   * 
   * @return
   */
  @PutMapping(value = "/stop", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> stop() {
    var status = global.getStatus();
    if (status == NlpStatus.running || status == NlpStatus.paused) {
      var event = new NlpStopEvent(this);
      applicationEventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(global), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(global), HttpStatus.BAD_REQUEST);
  }

  /**
   * Request the kafka consumer to pause.
   * 
   * @return
   */
  @PutMapping(value = "/pause", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> pause() {
    if (global.getStatus() == NlpStatus.running) {
      var event = new ConsumerPauseEvent(this);
      applicationEventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(global), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(global), HttpStatus.BAD_REQUEST);
  }

  /**
   * Request the kafka consumer to resume.
   * 
   * @return
   */
  @PutMapping(value = "/resume", produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReport> resume() {
    if (global.getStatus() == NlpStatus.paused) {
      var event = new ConsumerResumeEvent(this);
      applicationEventPublisher.publishEvent(event);
      return new ResponseEntity<>(new HealthReport(global), HttpStatus.OK);
    }

    return new ResponseEntity<>(new HealthReport(global), HttpStatus.BAD_REQUEST);
  }
}
