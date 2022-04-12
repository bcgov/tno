package ca.bc.gov.tno.areas.admin.controllers;

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

import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;
import ca.bc.gov.tno.models.Paged;
import ca.bc.gov.tno.areas.admin.models.DataSourceModel;

/**
 * Endpoints to communicate with the TNO DB data sources.
 */
@RolesAllowed("administrator")
@RestController("AdminDataSourceController")
@RequestMapping({ "/admin/data/sources", "/api/admin/data/sources" })
public class DataSourceController {

  @Autowired
  private IDataSourceService dataSourceService;

  /**
   * Request a list of all users from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Paged<DataSourceModel> find() {
    var result = dataSourceService.findAll().stream().map(i -> new DataSourceModel(i)).toList();
    return new Paged<DataSourceModel>(result, 1, result.size(), result.size());
  }

  /**
   * Request a list of all data sources from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public DataSourceModel findById(@PathVariable(required = true) Integer id) {
    var result = dataSourceService.findById(id).orElse(null);
    return new DataSourceModel(result);
  }

  /**
   * Add a new data source to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public DataSourceModel add(@RequestBody DataSourceModel model) {
    var result = dataSourceService.add(model.toDataSource());
    return new DataSourceModel(result);
  }

  /**
   * Update the data source in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public DataSourceModel update(@PathVariable Integer id, @RequestBody DataSourceModel model) {
    var result = dataSourceService.update(model.toDataSource());
    return new DataSourceModel(result);
  }

  /**
   * Delete the data source from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public DataSourceModel delete(@PathVariable Integer id, @RequestBody DataSourceModel model) {
    dataSourceService.delete(model.toDataSource());
    return model;
  }

}
