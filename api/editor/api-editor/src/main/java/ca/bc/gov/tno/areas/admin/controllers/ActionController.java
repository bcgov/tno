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

import ca.bc.gov.tno.dal.db.services.interfaces.IActionService;
import ca.bc.gov.tno.dal.db.entities.Action;

/**
 * Endpoints to communicate with the TNO DB actions.
 */
@RolesAllowed({ "administrator" })
@RestController("AdminActionController")
@RequestMapping({ "/admin/actions", "/api/admin/actions" })
public class ActionController {

  /**
   * DAL for action.
   */
  @Autowired
  private IActionService actionService;

  /**
   * Request a list of all actions from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<Action> findAll() {
    var results = actionService.findAll();
    return results;
  }

  /**
   * Request a list of all actions from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public Action findById(@PathVariable(required = true) Integer id) {
    var Action = actionService.findById(id).orElse(null);
    return Action;
  }

  /**
   * Add a new action to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Action add(@RequestBody Action model) {
    var action = actionService.add(model);
    return action;
  }

  /**
   * Update the action in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Action update(@PathVariable Integer id, @RequestBody Action model) {
    var action = actionService.add(model);
    return action;
  }

  /**
   * Delete the action from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Action delete(@PathVariable Integer id, @RequestBody Action model) {
    actionService.delete(model);
    return model;
  }

}
