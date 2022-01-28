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

import ca.bc.gov.tno.dal.db.services.interfaces.IContentTypeService;
import ca.bc.gov.tno.dal.db.entities.ContentType;

/**
 * Endpoints to communicate with the TNO DB contentTypes.
 */
@RolesAllowed({ "administrator" })
@RestController("AdminContentTypeController")
@RequestMapping("/admin/content/types")
public class ContentTypeController {

  /**
   * DAL for contentType.
   */
  @Autowired
  private IContentTypeService contentTypeService;

  /**
   * Request a list of all contentTypes from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<ContentType> findAll() {
    var results = contentTypeService.findAll();
    return results;
  }

  /**
   * Request a list of all contentTypes from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public ContentType findById(@PathVariable(required = true) Integer id) {
    var ContentType = contentTypeService.findById(id).orElse(null);
    return ContentType;
  }

  /**
   * Add a new contentType to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public ContentType add(@RequestBody ContentType model) {
    var contentType = contentTypeService.add(model);
    return contentType;
  }

  /**
   * Update the contentType in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public ContentType update(@PathVariable Integer id, @RequestBody ContentType model) {
    var contentType = contentTypeService.add(model);
    return contentType;
  }

  /**
   * Delete the contentType from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public ContentType delete(@PathVariable Integer id, @RequestBody ContentType model) {
    contentTypeService.delete(model);
    return model;
  }

}
