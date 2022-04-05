package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.ISourceMetricService;
import ca.bc.gov.tno.areas.editor.models.SourceMetricModel;

/**
 * SourceMetricController class, provides endpoints for metrics.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorSourceMetricController")
@RequestMapping({ "/editor/source/metrics", "/api/editor/source/metrics" })
public class SourceMetricController {

  /**
   * DAL for metric.
   */
  private ISourceMetricService metricService;

  /**
   * Creates a new instance of a SourceMetricController object, initializes with
   * specified parameters.
   *
   * @param metricService SourceMetric service.
   */
  @Autowired
  public SourceMetricController(ISourceMetricService metricService) {
    this.metricService = metricService;
  }

  /**
   * Request a list of all metrics from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<SourceMetricModel> findAll() {
    var results = metricService.findAll();
    var models = results.stream().map(a -> new SourceMetricModel(a)).toList();
    return models;
  }

}
