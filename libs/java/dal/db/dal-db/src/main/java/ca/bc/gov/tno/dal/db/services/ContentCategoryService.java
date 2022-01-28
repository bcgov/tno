package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentCategory;
import ca.bc.gov.tno.dal.db.entities.ContentCategoryPK;
import ca.bc.gov.tno.dal.db.repositories.IContentCategoryRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentCategoryService;

/**
 * ContentCategoryService class, provides a concrete way to interact with
 * content
 * types in the database.
 */
@Service
public class ContentCategoryService implements IContentCategoryService {

  private final IContentCategoryRepository repository;

  /**
   * Creates a new instance of a ContentCategoryService object, initializes with
   * specified parameters.
   * 
   * @param repository The content category repository.
   */
  @Autowired
  public ContentCategoryService(final IContentCategoryRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content category.
   */
  @Override
  public List<ContentCategory> findAll() {
    var ContentCategorys = (List<ContentCategory>) repository.findAll();
    return ContentCategorys;
  }

  /**
   * Find the content category for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content category if it exists.
   */
  @Override
  public Optional<ContentCategory> findById(ContentCategoryPK key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new content category to the data source.
   * 
   * @param entity The content category to add.
   * @return A new instance of the content category that was added.
   */
  @Override
  public ContentCategory add(ContentCategory entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified content category in the data source.
   * 
   * @param entity The content category to update.
   * @return A new instance of the content category that was updated.
   */
  @Override
  public ContentCategory update(ContentCategory entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content category from the data source.
   * 
   * @param entity The content category to delete.
   */
  @Override
  public void delete(ContentCategory entity) {
    repository.delete(entity);
  }

}
