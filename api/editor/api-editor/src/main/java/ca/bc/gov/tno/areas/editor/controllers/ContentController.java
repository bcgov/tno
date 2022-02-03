package ca.bc.gov.tno.areas.editor.controllers;

import java.util.Date;

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
import ca.bc.gov.tno.dal.db.ContentStatus;
import ca.bc.gov.tno.dal.db.entities.Content;
import ca.bc.gov.tno.dal.db.models.FilterCollection;
import ca.bc.gov.tno.dal.db.models.LogicalOperators;
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
      @RequestParam(required = false) Integer quantity,
      @RequestParam(required = false) String pageName,
      @RequestParam(required = false) String section,
      @RequestParam(required = false) String headline,
      @RequestParam(required = false) Integer mediaTypeId,
      @RequestParam(required = false) Integer contentTypeId,
      @RequestParam(required = false) String source,
      @RequestParam(required = false) Integer dataSourceId,
      @RequestParam(required = false) Integer ownerId,
      @RequestParam(required = false) ContentStatus status,
      @RequestParam(required = false) Date createdOn,
      @RequestParam(required = false) Date createdStartOn,
      @RequestParam(required = false) Date createdEndOn,
      @RequestParam(required = false) Date updatedOn,
      @RequestParam(required = false) Date updatedStartOn,
      @RequestParam(required = false) Date updatedEndOn,
      @RequestParam(required = false) Date publishedOn,
      @RequestParam(required = false) Date publishedStartOn,
      @RequestParam(required = false) Date publishedEndOn,
      @RequestParam(required = false) Boolean hasPage,
      @RequestParam(required = false) String actions,
      @RequestParam(required = false) LogicalOperators logicalOperator) {

    var filter = new FilterCollection();
    if (hasPage != null && hasPage == true)
      filter.addFilter("page", LogicalOperators.NotEqual, "");
    if (pageName != null)
      filter.addFilter("page", logicalOperator, pageName);
    if (section != null)
      filter.addFilter("section", logicalOperator, section);
    if (headline != null)
      filter.addFilter("headline", logicalOperator, headline);
    if (mediaTypeId != null)
      filter.addFilter("mediaTypeId", logicalOperator, mediaTypeId);
    if (contentTypeId != null)
      filter.addFilter("contentTypeId", logicalOperator, contentTypeId);
    if (source != null)
      filter.addFilter("source", logicalOperator, source);
    if (dataSourceId != null)
      filter.addFilter("dataSourceId", logicalOperator, dataSourceId);
    if (ownerId != null)
      filter.addFilter("ownerId", logicalOperator, ownerId);
    if (status != null)
      filter.addFilter("status", logicalOperator, status.getValue());
    if (createdOn != null)
      filter.addFilter("createdOn", logicalOperator, createdOn);
    if (createdStartOn != null)
      filter.addFilter("createdOn", LogicalOperators.GreaterThanOrEqual, createdStartOn);
    if (createdEndOn != null)
      filter.addFilter("createdOn", LogicalOperators.LessThanOrEqual, createdEndOn);
    if (updatedOn != null)
      filter.addFilter("updatedOn", logicalOperator, updatedOn);
    if (updatedStartOn != null)
      filter.addFilter("updatedOn", LogicalOperators.GreaterThanOrEqual, updatedStartOn);
    if (updatedEndOn != null)
      filter.addFilter("updatedOn", LogicalOperators.LessThanOrEqual, updatedEndOn);
    if (publishedOn != null)
      filter.addFilter("publishedOn", logicalOperator, publishedOn);
    if (publishedStartOn != null)
      filter.addFilter("publishedOn", LogicalOperators.GreaterThanOrEqual, publishedStartOn);
    if (publishedEndOn != null)
      filter.addFilter("publishedOn", LogicalOperators.LessThanOrEqual, publishedEndOn);

    var results = contentService.find(page == null ? 1 : page, quantity == null ? 10 : quantity, filter, null);
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
