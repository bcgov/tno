package ca.bc.gov.tno.areas.editor.controllers;

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
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IContentService;
import ca.bc.gov.tno.areas.editor.models.ContentModel;
import ca.bc.gov.tno.dal.db.entities.Content;
import ca.bc.gov.tno.models.Paged;
import ca.bc.gov.tno.models.interfaces.IPaged;

/**
 * Endpoints to communicate with the TNO DB contents.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorContentController")
@RequestMapping("/editor/contents")
public class ContentController {

  /**
   * DAL for content.
   */
  @Autowired
  private IContentService contentService;

  /**
   * Request a list of all contents from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public IPaged<ContentModel> findAll(
      @RequestParam(required = false) Integer page,
      @RequestParam(required = false) Integer quantity) {
    var results = contentService.find(page == null ? 1 : page, quantity == null ? 10 : quantity, null);
    var paged = new Paged<ContentModel>(
        results.getItems().stream().map(c -> new ContentModel(c)).toList(),
        results.getPage(),
        results.getQuantity(),
        results.getTotal());

    return paged;
  }

  /**
   * Request a list of all contents from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public Content findById(@PathVariable(required = true) Integer id) {
    var Content = contentService.findById(id).orElse(null);
    return Content;
  }

  /**
   * Add a new content to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE,
      MediaType.APPLICATION_ATOM_XML_VALUE,
      MediaType.APPLICATION_JSON_VALUE + ";charset=UTF-8" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Content add(@RequestBody ContentModel model) {
    var content = contentService.add(model.Convert());
    return content;
  }

  /**
   * Update the content in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Content update(@PathVariable Integer id, @RequestBody Content model) {
    var content = contentService.add(model);
    return content;
  }

  /**
   * Delete the content from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Content delete(@PathVariable Integer id, @RequestBody Content model) {
    contentService.delete(model);
    return model;
  }

}
