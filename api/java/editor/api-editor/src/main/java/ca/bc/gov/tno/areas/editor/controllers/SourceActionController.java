package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.ISourceActionService;
import ca.bc.gov.tno.areas.editor.models.SourceActionModel;

/**
 * SourceActionController class, provides endpoints for actions.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorSourceActionController")
@RequestMapping({ "/editor/source/actions", "/api/editor/source/actions" })
public class SourceActionController {

  /**
   * DAL for action.
   */
  private ISourceActionService actionService;

  /**
   * Creates a new instance of a SourceActionController object, initializes with
   * specified parameters.
   *
   * @param actionService SourceAction service.
   */
  @Autowired
  public SourceActionController(ISourceActionService actionService) {
    this.actionService = actionService;
  }

  /**
   * Request a list of all actions from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<SourceActionModel> findAll() {
    var results = actionService.findAll();
    var models = results.stream().map(a -> new SourceActionModel(a)).toList();
    return models;
  }

}
