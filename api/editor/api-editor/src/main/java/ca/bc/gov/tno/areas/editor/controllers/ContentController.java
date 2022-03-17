package ca.bc.gov.tno.areas.editor.controllers;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
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
import ca.bc.gov.tno.dal.db.WorkflowStatus;
import ca.bc.gov.tno.dal.db.models.FilterCollection;
import ca.bc.gov.tno.dal.db.models.LogicalOperators;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.models.Paged;
import ca.bc.gov.tno.models.interfaces.IPaged;

/**
 * Endpoints to communicate with the TNO DB contents.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorContentController")
@RequestMapping({ "/editor/contents", "/api/editor/contents" })
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
  public IPaged<ContentModel> find(
      @RequestParam(required = false) Integer page,
      @RequestParam(required = false) Integer quantity,
      @RequestParam(required = false) LogicalOperators logicalOperator,
      @RequestParam(required = false) ContentStatus status,
      @RequestParam(required = false) WorkflowStatus workflowStatus,
      @RequestParam(required = false) Integer contentTypeId,
      @RequestParam(required = false) Integer mediaTypeId,
      @RequestParam(required = false) Integer ownerId,
      @RequestParam(required = false) Integer userId,
      @RequestParam(required = false) Integer dataSourceId,
      @RequestParam(required = false) String source,
      @RequestParam(required = false) String headline,
      @RequestParam(required = false) String pageName,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Date createdOn,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Date createdStartOn,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Date createdEndOn,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Date updatedOn,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Date updatedStartOn,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Date updatedEndOn,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Date publishedOn,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Date publishedStartOn,
      @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) Date publishedEndOn,
      @RequestParam(required = false) Boolean hasPage,
      @RequestParam(required = false) String actions,
      @RequestParam(required = false) String edition,
      @RequestParam(required = false) String section,
      @RequestParam(required = false) String storyType,
      @RequestParam(required = false) String byline,
      @RequestParam(required = false) String sort) {

    var filter = new FilterCollection();
    if (hasPage != null && hasPage == true)
      filter.addFilter("page", LogicalOperators.NotEqual, "");
    if (pageName != null)
      filter.addFilter("page", logicalOperator, pageName);
    if (status != null)
      filter.addFilter("status", logicalOperator, status.getValue());
    if (workflowStatus != null)
      filter.addFilter("workflowStatus", logicalOperator, workflowStatus.getValue());
    if (contentTypeId != null)
      filter.addFilter("contentTypeId", logicalOperator, contentTypeId);
    if (mediaTypeId != null)
      filter.addFilter("mediaTypeId", logicalOperator, mediaTypeId);
    if (ownerId != null)
      filter.addFilter("ownerId", logicalOperator, ownerId);
    if (userId != null)
      filter.addFilter("timeTracking", "userId", LogicalOperators.Equals, userId);
    if (dataSourceId != null)
      filter.addFilter("dataSourceId", logicalOperator, dataSourceId);
    if (source != null)
      filter.addFilter("source", logicalOperator, source);
    if (headline != null)
      filter.addFilter("headline", logicalOperator, headline);
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

    // Actions
    if (actions != null) {
      var actionArray = actions.split(",");
      for (var action : actionArray) {
        filter.addFilter("action", "name", LogicalOperators.Equals, action);
      }
    }

    // Print Content filters.
    if (section != null)
      filter.addFilter("print", "section", logicalOperator, section);
    if (edition != null)
      filter.addFilter("print", "edition", logicalOperator, edition);
    if (storyType != null)
      filter.addFilter("print", "storyType", logicalOperator, storyType);
    if (byline != null)
      filter.addFilter("print", "byline", logicalOperator, byline);

    // Sort By
    var sortBy = new ArrayList<SortParam>();
    if (sort != null && sort.length() > 0)
      Arrays.stream(sort.split(",")).forEach((sb) -> {
        var colDir = sb.split(" ");
        var dir = colDir.length == 2 ? colDir[1] : "";
        var table = "content";
        if (colDir[0].equals("section"))
          table = "print";
        sortBy.add(new SortParam(table, colDir[0], dir));
      });

    var results = contentService.find(page == null ? 1 : page, quantity == null ? 10 : quantity, filter,
        sortBy.toArray(SortParam[]::new));
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
  public ResponseEntity<ContentModel> findById(@PathVariable(required = true) Integer id) {
    var content = contentService.findById(id, true).orElse(null);
    if (content == null)
      return new ResponseEntity<ContentModel>(HttpStatus.NO_CONTENT);
    return new ResponseEntity<ContentModel>(new ContentModel(content), HttpStatus.OK);
  }

  /**
   * Add a new content to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public ContentModel add(@RequestBody ContentModel model) {
    var content = contentService.add(model.ToContent());
    return new ContentModel(content);
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
  public ContentModel update(@PathVariable Integer id, @RequestBody ContentModel model) {
    var content = contentService.update(model.ToContent());
    return new ContentModel(content);
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
  public ContentModel delete(@PathVariable Integer id, @RequestBody ContentModel model) {
    contentService.delete(model.ToContent());
    return model;
  }

}
