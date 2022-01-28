package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IActionService;
import ca.bc.gov.tno.dal.db.entities.Action;

/**
 * ActionController class, provides endpoints for actions.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorActionController")
@RequestMapping("/editor/actions")
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

}
