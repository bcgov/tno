package ca.bc.gov.tno.areas.editor.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.ICategoryService;
import ca.bc.gov.tno.dal.db.entities.Category;

/**
 * Endpoints to communicate with the TNO DB categories.
 */
@RolesAllowed({ "administrator", "editor" })
@RestController("EditorCategoryController")
@RequestMapping({ "/editor/categories", "/api/editor/categories" })
public class CategoryController {

  /**
   * DAL for category.
   */
  @Autowired
  private ICategoryService categoryService;

  /**
   * Request a list of all categories from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<Category> findAll() {
    var results = categoryService.findAll();
    return results;
  }

}
