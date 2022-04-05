package ca.bc.gov.tno.areas.admin.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.ISourceMetricService;
import ca.bc.gov.tno.dal.db.entities.SourceMetric;

/**
 * Endpoints to communicate with the TNO DB sourceMetrics.
 */
@RolesAllowed({ "administrator" })
@RestController("AdminSourceMetricController")
@RequestMapping({ "/admin/source/metrics", "/api/admin/source/metrics" })
public class SourceMetricController {

  /**
   * DAL for sourceMetric.
   */
  @Autowired
  private ISourceMetricService sourceMetricService;

  /**
   * Request a list of all sourceMetrics from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<SourceMetric> findAll() {
    var results = sourceMetricService.findAll();
    return results;
  }

  /**
   * Request a list of all sourceMetrics from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public SourceMetric findById(@PathVariable(required = true) Integer id) {
    var SourceMetric = sourceMetricService.findById(id).orElse(null);
    return SourceMetric;
  }

  /**
   * Add a new sourceMetric to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public SourceMetric add(@RequestBody SourceMetric model) {
    var sourceMetric = sourceMetricService.add(model);
    return sourceMetric;
  }

  /**
   * Update the sourceMetric in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public SourceMetric update(@PathVariable Integer id, @RequestBody SourceMetric model) {
    var sourceMetric = sourceMetricService.update(model);
    return sourceMetric;
  }

  /**
   * Delete the sourceMetric from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public SourceMetric delete(@PathVariable Integer id, @RequestBody SourceMetric model) {
    sourceMetricService.delete(model);
    return model;
  }

}
