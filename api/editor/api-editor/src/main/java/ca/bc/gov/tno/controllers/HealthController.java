package ca.bc.gov.tno.controllers;

import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.models.health.HealthReportModel;

@RestController
@RequestMapping("/health")
public class HealthController {

  @GetMapping(value = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public ResponseEntity<HealthReportModel> health() {
    return new ResponseEntity<>(new HealthReportModel("running"), HttpStatus.OK);
  }
}
