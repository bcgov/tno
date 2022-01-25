package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentAction;
import ca.bc.gov.tno.dal.db.entities.ContentActionPK;
import ca.bc.gov.tno.dal.db.repositories.IContentActionRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentActionService;

/**
 * ContentActionService class, provides a concrete way to interact with
 * content
 * types in the database.
 */
@Service
public class ContentActionService implements IContentActionService {

  private final IContentActionRepository repository;

  /**
   * Creates a new instance of a ContentActionService object, initializes with
   * specified parameters.
   * 
   * @param repository The content action repository.
   */
  @Autowired
  public ContentActionService(final IContentActionRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content action.
   */
  @Override
  public List<ContentAction> findAll() {
    var ContentActions = (List<ContentAction>) repository.findAll();
    return ContentActions;
  }

  /**
   * Find the content action for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content action if it exists.
   */
  @Override
  public Optional<ContentAction> findById(ContentActionPK key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new content action to the data source.
   * 
   * @param entity The content action to add.
   * @return A new instance of the content action that was added.
   */
  @Override
  public ContentAction add(ContentAction entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified content action in the data source.
   * 
   * @param entity The content action to update.
   * @return A new instance of the content action that was updated.
   */
  @Override
  public ContentAction update(ContentAction entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content action from the data source.
   * 
   * @param entity The content action to delete.
   */
  @Override
  public void delete(ContentAction entity) {
    repository.delete(entity);
  }

}
