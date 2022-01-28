package ca.bc.gov.tno.areas.admin.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IMediaTypeService;
import ca.bc.gov.tno.dal.db.entities.MediaType;

/**
 * Endpoints to communicate with the TNO DB data source types.
 */
@RolesAllowed("administrator")
@RestController("AdminMediaTypeController")
@RequestMapping("/admin/media/types")
public class MediaTypeController {

  @Autowired
  private IMediaTypeService mediaTypeService;

  /**
   * Request a list of all data source types from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public List<MediaType> findAll() {
    var mediaType = mediaTypeService.findAll();
    return mediaType;
  }

  /**
   * Request a list of all data source types from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public MediaType findById(@PathVariable(required = true) Integer id) {
    var mediaType = mediaTypeService.findById(id).orElse(null);
    return mediaType;
  }

  /**
   * Add a new user to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = org.springframework.http.MediaType.APPLICATION_JSON_VALUE, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public MediaType add(@RequestBody MediaType model) {
    var mediaType = mediaTypeService.add(model);
    return mediaType;
  }

  /**
   * Update the user in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = org.springframework.http.MediaType.APPLICATION_JSON_VALUE, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public MediaType update(@PathVariable Integer id, @RequestBody MediaType model) {
    var mediaType = mediaTypeService.add(model);
    return mediaType;
  }

  /**
   * Delete the user from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = org.springframework.http.MediaType.APPLICATION_JSON_VALUE, produces = org.springframework.http.MediaType.APPLICATION_JSON_VALUE)
  public MediaType delete(@PathVariable Integer id, @RequestBody MediaType model) {
    mediaTypeService.delete(model);
    return model;
  }

}
