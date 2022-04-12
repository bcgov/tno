package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.ITagService;
import ca.bc.gov.tno.areas.editor.models.TagModel;

/**
 * Endpoints to communicate with the TNO DB tags.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorTagController")
@RequestMapping({ "/editor/tags", "/api/editor/tags" })
public class TagController {

  /**
   * DAL for tag.
   */
  @Autowired
  private ITagService tagService;

  /**
   * Request a list of all tags from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<TagModel> findAll() {
    var results = tagService.findAll();
    var models = results.stream().map(t -> new TagModel(t)).toList();
    return models;
  }

}
