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

import ca.bc.gov.tno.dal.db.services.interfaces.ISourceActionService;
import ca.bc.gov.tno.dal.db.entities.SourceAction;

/**
 * Endpoints to communicate with the TNO DB sourceActions.
 */
@RolesAllowed({ "administrator" })
@RestController("AdminSourceActionController")
@RequestMapping({ "/admin/source/actions", "/api/admin/source/actions" })
public class SourceActionController {

  /**
   * DAL for sourceAction.
   */
  @Autowired
  private ISourceActionService sourceActionService;

  /**
   * Request a list of all sourceActions from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<SourceAction> findAll() {
    var results = sourceActionService.findAll();
    return results;
  }

  /**
   * Request a list of all sourceActions from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public SourceAction findById(@PathVariable(required = true) Integer id) {
    var SourceAction = sourceActionService.findById(id).orElse(null);
    return SourceAction;
  }

  /**
   * Add a new sourceAction to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public SourceAction add(@RequestBody SourceAction model) {
    var sourceAction = sourceActionService.add(model);
    return sourceAction;
  }

  /**
   * Update the sourceAction in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public SourceAction update(@PathVariable Integer id, @RequestBody SourceAction model) {
    var sourceAction = sourceActionService.update(model);
    return sourceAction;
  }

  /**
   * Delete the sourceAction from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public SourceAction delete(@PathVariable Integer id, @RequestBody SourceAction model) {
    sourceActionService.delete(model);
    return model;
  }

}
