package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentType;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IContentTypeRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentTypeService;

/**
 * ContentTypeService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class ContentTypeService implements IContentTypeService {

  private final IContentTypeRepository repository;

  /**
   * Creates a new instance of a ContentTypeService object, initializes with
   * specified parameters.
   * 
   * @param repository The content type repository.
   */
  @Autowired
  public ContentTypeService(final IContentTypeRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content type.
   */
  @Override
  public List<ContentType> findAll() {
    var result = (List<ContentType>) repository.findAll();
    return result;
  }

  /**
   * Find the content type for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content type if it exists.
   */
  @Override
  public Optional<ContentType> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Add a new content type to the data source.
   * 
   * @param entity The content type to add.
   * @return A new instance of the content type that was added.
   */
  @Override
  public ContentType add(ContentType entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified content type in the data source.
   * 
   * @param entity The content type to update.
   * @return A new instance of the content type that was updated.
   */
  @Override
  public ContentType update(ContentType entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content type from the data source.
   * 
   * @param entity The content type to delete.
   */
  @Override
  public void delete(ContentType entity) {
    repository.delete(entity);
  }

}
