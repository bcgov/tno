package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;
import ca.bc.gov.tno.dal.db.repositories.IContentReferenceRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentReferenceService;

/**
 * ContentReferenceService class, provides a concrete way to interact with
 * content references in the database.
 */
@Service
public class ContentReferenceService implements IContentReferenceService {

  private final IContentReferenceRepository repository;

  /**
   * Creates a new instance of a ContentReferenceService object, initializes with
   * specified parameters.
   * 
   * @param repository The content reference repository.
   */
  @Autowired
  public ContentReferenceService(final IContentReferenceRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content reference.
   */
  @Override
  public List<ContentReference> findAll() {
    var references = (List<ContentReference>) repository.findAll();
    return references;
  }

  /**
   * Find the content reference for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content reference if it exists.
   */
  @Override
  public Optional<ContentReference> findById(ContentReferencePK key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new content reference to the data source.
   * 
   * @param entity The content reference to add.
   * @return A new instance of the content reference that was added.
   */
  @Override
  public ContentReference add(ContentReference entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified content reference in the data source.
   * 
   * @param entity The content reference to update.
   * @return A new instance of the content reference that was updated.
   */
  @Override
  public ContentReference update(ContentReference entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content reference from the data source.
   * 
   * @param entity The content reference to delete.
   */
  @Override
  public void delete(ContentReference entity) {
    repository.delete(entity);
  }

}
