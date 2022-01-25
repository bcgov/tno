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

import ca.bc.gov.tno.dal.db.services.interfaces.IContentReferenceService;
import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;

/**
 * Endpoints to communicate with the TNO DB content references.
 */
@RolesAllowed("administrator")
@RestController("AdminContentReferenceController")
@RequestMapping("/admin/content/references")
public class ContentReferenceController {

  @Autowired
  private IContentReferenceService contentReferenceService;

  /**
   * Request a list of all content references from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<ContentReference> findAll() {
    var contentReferences = contentReferenceService.findAll();
    return contentReferences;
  }

  /**
   * Request a list of all content references from the db.
   *
   * @param source The data source identifier.
   * @param uid    The unique content identifier.
   * @return
   */
  @GetMapping(path = "/{source}/{uid}", produces = MediaType.APPLICATION_JSON_VALUE)
  public ContentReference findById(@PathVariable(required = true) String source,
      @PathVariable(required = true) String uid) {
    var contentReference = contentReferenceService.findById(new ContentReferencePK(source, uid)).orElse(null);
    return contentReference;
  }

  /**
   * Add a new contentReference to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = "/", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
  public ContentReference add(@RequestBody ContentReference model) {
    var contentReference = contentReferenceService.add(model);
    return contentReference;
  }

  /**
   * Update the contentReference in the db.
   *
   * @param source The data source identifier.
   * @param uid    The unique content identifier.
   * @param model
   * @return
   */
  @PutMapping(path = "/{source}/{uid}", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
  public ContentReference update(@PathVariable(required = true) String source,
      @PathVariable(required = true) String uid, @RequestBody ContentReference model) {
    var contentReference = contentReferenceService.add(model);
    return contentReference;
  }

  /**
   * Delete the contentReference from the db.
   *
   * @param source The data source identifier.
   * @param uid    The unique content identifier.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{source}/{uid}", consumes = MediaType.APPLICATION_JSON_VALUE, produces = MediaType.APPLICATION_JSON_VALUE)
  public ContentReference delete(@PathVariable(required = true) String source,
      @PathVariable(required = true) String uid, @RequestBody ContentReference model) {
    contentReferenceService.delete(model);
    return model;
  }

}
