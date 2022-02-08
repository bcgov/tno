package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.ISeriesService;
import ca.bc.gov.tno.areas.editor.models.SeriesModel;

/**
 * Endpoints to communicate with the TNO DB series.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorSeriesController")
@RequestMapping({ "/editor/series", "/api/editor/series" })
public class SeriesController {

  /**
   * DAL for series.
   */
  @Autowired
  private ISeriesService seriesService;

  /**
   * Request a list of all series from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public List<SeriesModel> findAll() {
    var results = seriesService.findAll();
    var models = results.stream().map(m -> new SeriesModel(m)).toList();
    return models;
  }

}
