package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IDataLocationService;
import ca.bc.gov.tno.areas.editor.models.DataLocationModel;

/**
 * Endpoints to communicate with the TNO DB data locations.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorDataLocationController")
@RequestMapping({ "/editor/data/locations", "/api/editor/data/locations" })
public class DataLocationController {

  /**
   * DAL for data location.
   */
  @Autowired
  private IDataLocationService mediaTypeService;

  /**
   * Request a list of all data locations from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public List<DataLocationModel> findAll() {
    var results = mediaTypeService.findAll();
    var models = results.stream().map(m -> new DataLocationModel(m)).toList();
    return models;
  }

}
