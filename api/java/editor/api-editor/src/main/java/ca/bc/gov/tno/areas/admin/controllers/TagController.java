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

import ca.bc.gov.tno.dal.db.services.interfaces.ITagService;
import ca.bc.gov.tno.dal.db.entities.Tag;

/**
 * Endpoints to communicate with the TNO DB tags.
 */
@RolesAllowed({ "administrator" })
@RestController("AdminTagController")
@RequestMapping({ "/admin/tags", "/api/admin/tags" })
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
  public List<Tag> findAll() {
    var results = tagService.findAll();
    return results;
  }

  /**
   * Request a list of all tags from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public Tag findById(@PathVariable(required = true) String id) {
    var Tag = tagService.findById(id).orElse(null);
    return Tag;
  }

  /**
   * Add a new tag to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Tag add(@RequestBody Tag model) {
    var tag = tagService.add(model);
    return tag;
  }

  /**
   * Update the tag in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Tag update(@PathVariable Integer id, @RequestBody Tag model) {
    var tag = tagService.update(model);
    return tag;
  }

  /**
   * Delete the tag from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Tag delete(@PathVariable Integer id, @RequestBody Tag model) {
    tagService.delete(model);
    return model;
  }

}
