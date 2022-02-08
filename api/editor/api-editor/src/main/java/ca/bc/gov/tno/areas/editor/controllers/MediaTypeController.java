package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IMediaTypeService;
import ca.bc.gov.tno.areas.editor.models.MediaTypeModel;

/**
 * Endpoints to communicate with the TNO DB media types.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorMediaTypeController")
@RequestMapping({ "/editor/media/types", "/api/editor/media/types" })
public class MediaTypeController {

  /**
   * DAL for media type.
   */
  @Autowired
  private IMediaTypeService mediaTypeService;

  /**
   * Request a list of all media types from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public List<MediaTypeModel> findAll() {
    var results = mediaTypeService.findAll();
    var models = results.stream().map(m -> new MediaTypeModel(m)).toList();
    return models;
  }

}
