package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;
import ca.bc.gov.tno.areas.editor.models.UserModel;

/**
 * Endpoints to communicate with the TNO DB tone pools.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorUserController")
@RequestMapping("/editor/users")
public class UserController {

  /**
   * DAL for tone pools.
   */
  @Autowired
  private IUserService tonePoolService;

  /**
   * Request a list of all tone pools from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<UserModel> findAll() {
    var results = tonePoolService.findAll();
    var models = results.stream().map(t -> new UserModel(t)).toList();
    return models;
  }

}
