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

import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;
import ca.bc.gov.tno.dal.db.entities.User;

/**
 * Endpoints to communicate with the TNO DB users.
 */
@RolesAllowed("administrator")
@RestController("AdminUserController")
@RequestMapping({ "/admin/users", "/api/admin/users" })
public class UserController {

  @Autowired
  private IUserService userService;

  /**
   * Request a list of all users from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<User> findAll() {
    var users = userService.findAll();
    return users;
  }

  /**
   * Request a list of all users from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public User findById(@PathVariable(required = true) Integer id) {
    var user = userService.findById(id).orElse(null);
    return user;
  }

  /**
   * Add a new user to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public User add(@RequestBody User model) {
    var user = userService.add(model);
    return user;
  }

  /**
   * Update the user in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public User update(@PathVariable Integer id, @RequestBody User model) {
    var user = userService.add(model);
    return user;
  }

  /**
   * Delete the user from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public User delete(@PathVariable Integer id, @RequestBody User model) {
    userService.delete(model);
    return model;
  }

}
