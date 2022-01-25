package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IContentTypeService;
import ca.bc.gov.tno.dal.db.entities.ContentType;

/**
 * Endpoints to communicate with the TNO DB content types.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorContentTypeController")
@RequestMapping("/editor/content/types")
public class ContentTypeController {

  /**
   * DAL for content type.
   */
  @Autowired
  private IContentTypeService contentTypeService;

  /**
   * Request a list of all content types from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<ContentType> findAll() {
    var results = contentTypeService.findAll();
    return results;
  }

}
