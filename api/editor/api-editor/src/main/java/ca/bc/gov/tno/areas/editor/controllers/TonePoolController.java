package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.ITonePoolService;
import ca.bc.gov.tno.areas.editor.models.TonePoolModel;

/**
 * Endpoints to communicate with the TNO DB tone pools.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorTonePoolController")
@RequestMapping("/editor/tone/pools")
public class TonePoolController {

  /**
   * DAL for tone pools.
   */
  @Autowired
  private ITonePoolService tonePoolService;

  /**
   * Request a list of all tone pools from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<TonePoolModel> findAll() {
    var results = tonePoolService.findAll();
    var models = results.stream().map(t -> new TonePoolModel(t)).toList();
    return models;
  }

}
