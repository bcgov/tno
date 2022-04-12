package ca.bc.gov.tno.areas.admin.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IDataLocationService;
import ca.bc.gov.tno.dal.db.entities.DataLocation;

/**
 * Endpoints to communicate with the TNO DB data data locations.
 */
@RolesAllowed("administrator")
@RestController("AdminDataLocationController")
@RequestMapping({ "/admin/data/locations", "/api/admin/data/locations" })
public class DataLocationController {

  @Autowired
  private IDataLocationService dataLocationService;

  /**
   * Request a list of all data data locations from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public List<DataLocation> findAll() {
    var dataLocation = dataLocationService.findAll();
    return dataLocation;
  }

  /**
   * Request a list of all data data locations from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public DataLocation findById(@PathVariable(required = true) Integer id) {
    var dataLocation = dataLocationService.findById(id).orElse(null);
    return dataLocation;
  }

  /**
   * Add a new user to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "",
      "/" }, consumes = org.springframework.http.MediaType.APPLICATION_JSON_VALUE, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public DataLocation add(@RequestBody DataLocation model) {
    var dataLocation = dataLocationService.add(model);
    return dataLocation;
  }

  /**
   * Update the user in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = org.springframework.http.MediaType.APPLICATION_JSON_VALUE, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public DataLocation update(@PathVariable Integer id, @RequestBody DataLocation model) {
    var dataLocation = dataLocationService.update(model);
    return dataLocation;
  }

  /**
   * Delete the user from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = org.springframework.http.MediaType.APPLICATION_JSON_VALUE, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public DataLocation delete(@PathVariable Integer id, @RequestBody DataLocation model) {
    dataLocationService.delete(model);
    return model;
  }

}
