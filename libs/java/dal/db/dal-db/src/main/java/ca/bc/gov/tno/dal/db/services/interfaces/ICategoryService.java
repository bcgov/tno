package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Category;

/**
 * ICategoryService interface, provides a way to interact with content
 * types.
 */
public interface ICategoryService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of category.
   */
  List<Category> findAll();

  /**
   * Find the category for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the category if it exists.
   */
  Optional<Category> findById(int key);

  /**
   * Add a new category to the content.
   * 
   * @param entity The category to add.
   * @return A new instance of the category that was added.
   */
  Category add(Category entity);

  /**
   * Update the specified category in the content.
   * 
   * @param entity The category to update.
   * @return A new instance of the category that was updated.
   */
  Category update(Category entity);

  /**
   * Delete the specified category from the content.
   * 
   * @param entity The category to delete.
   */
  void delete(Category entity);
}
