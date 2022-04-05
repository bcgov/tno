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

import ca.bc.gov.tno.dal.db.services.interfaces.ISeriesService;
import ca.bc.gov.tno.dal.db.entities.Series;

/**
 * Endpoints to communicate with the TNO DB series.
 */
@RolesAllowed({ "administrator" })
@RestController("AdminSeriesController")
@RequestMapping({ "/admin/series", "/api/admin/series" })
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
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<Series> findAll() {
    var results = seriesService.findAll();
    return results;
  }

  /**
   * Request a list of all series from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public Series findById(@PathVariable(required = true) int id) {
    var Series = seriesService.findById(id).orElse(null);
    return Series;
  }

  /**
   * Add a new series to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Series add(@RequestBody Series model) {
    var series = seriesService.add(model);
    return series;
  }

  /**
   * Update the series in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Series update(@PathVariable Integer id, @RequestBody Series model) {
    var series = seriesService.update(model);
    return series;
  }

  /**
   * Delete the series from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Series delete(@PathVariable Integer id, @RequestBody Series model) {
    seriesService.delete(model);
    return model;
  }

}
