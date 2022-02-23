package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Category;
import ca.bc.gov.tno.dal.db.repositories.interfaces.ICategoryRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ICategoryService;

/**
 * CategoryService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class CategoryService implements ICategoryService {

  private final ICategoryRepository repository;

  /**
   * Creates a new instance of a CategoryService object, initializes with
   * specified parameters.
   * 
   * @param repository The category repository.
   */
  @Autowired
  public CategoryService(final ICategoryRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of category.
   */
  @Override
  public List<Category> findAll() {
    var result = (List<Category>) repository.findAll();
    return result;
  }

  /**
   * Find the category for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the category if it exists.
   */
  @Override
  public Optional<Category> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Add a new category to the data source.
   * 
   * @param entity The category to add.
   * @return A new instance of the category that was added.
   */
  @Override
  public Category add(Category entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified category in the data source.
   * 
   * @param entity The category to update.
   * @return A new instance of the category that was updated.
   */
  @Override
  public Category update(Category entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified category from the data source.
   * 
   * @param entity The category to delete.
   */
  @Override
  public void delete(Category entity) {
    repository.delete(entity);
  }

}
