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

import ca.bc.gov.tno.dal.db.services.interfaces.ICategoryService;
import ca.bc.gov.tno.dal.db.entities.Category;

/**
 * Endpoints to communicate with the TNO DB categories.
 */
@RolesAllowed({ "administrator" })
@RestController("AdminCategoryController")
@RequestMapping({ "/admin/categories", "/api/admin/categories" })
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

  /**
   * Request a list of all categories from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public Category findById(@PathVariable(required = true) Integer id) {
    var Category = categoryService.findById(id).orElse(null);
    return Category;
  }

  /**
   * Add a new category to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Category add(@RequestBody Category model) {
    var category = categoryService.add(model);
    return category;
  }

  /**
   * Update the category in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Category update(@PathVariable Integer id, @RequestBody Category model) {
    var category = categoryService.update(model);
    return category;
  }

  /**
   * Delete the category from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Category delete(@PathVariable Integer id, @RequestBody Category model) {
    categoryService.delete(model);
    return model;
  }

}
