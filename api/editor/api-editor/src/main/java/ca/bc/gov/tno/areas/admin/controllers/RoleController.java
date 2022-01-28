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

import ca.bc.gov.tno.dal.db.services.interfaces.IRoleService;
import ca.bc.gov.tno.dal.db.entities.Role;

/**
 * Endpoints to communicate with the TNO DB roles.
 */
@RolesAllowed("administrator")
@RestController("AdminRoleController")
@RequestMapping("/admin/roles")
public class RoleController {

  @Autowired
  private IRoleService roleService;

  /**
   * Request a list of all roles from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<Role> findAll() {
    var roles = roleService.findAll();
    return roles;
  }

  /**
   * Request a list of all roles from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public Role findById(@PathVariable(required = true) Integer id) {
    var Role = roleService.findById(id).orElse(null);
    return Role;
  }

  /**
   * Add a new role to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Role add(@RequestBody Role model) {
    var role = roleService.add(model);
    return role;
  }

  /**
   * Update the role in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Role update(@PathVariable Integer id, @RequestBody Role model) {
    var role = roleService.add(model);
    return role;
  }

  /**
   * Delete the role from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Role delete(@PathVariable Integer id, @RequestBody Role model) {
    roleService.delete(model);
    return model;
  }

}
