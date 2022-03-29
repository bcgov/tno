package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.SourceAction;
import ca.bc.gov.tno.dal.db.repositories.interfaces.ISourceActionRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ISourceActionService;

/**
 * SourceActionService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class SourceActionService implements ISourceActionService {

  private final ISourceActionRepository repository;

  /**
   * Creates a new instance of a SourceActionService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The source action repository.
   */
  @Autowired
  public SourceActionService(final SessionFactory sessionFactory, final ISourceActionRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of sourceAction.
   */
  @Override
  public List<SourceAction> findAll() {
    var result = (List<SourceAction>) repository.findAll();
    return result;
  }

  /**
   * Find the sourceAction for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the sourceAction if it exists.
   */
  @Override
  public Optional<SourceAction> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find the sourceAction for the specified primary key.
   * 
   * @param name The name of the sourceAction.
   * @return A new instance of the sourceAction if it exists.
   */
  @Override
  public Optional<SourceAction> findByName(String name) {
    var result = repository.findByName(name);
    return result;
  }

  /**
   * Add a new sourceAction to the data source.
   * 
   * @param entity The sourceAction to add.
   * @return A new instance of the sourceAction that was added.
   */
  @Override
  public SourceAction add(SourceAction entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified sourceAction in the data source.
   * 
   * @param entity The sourceAction to update.
   * @return A new instance of the sourceAction that was updated.
   */
  @Override
  public SourceAction update(SourceAction entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified sourceAction from the data source.
   * 
   * @param entity The sourceAction to delete.
   */
  @Override
  public void delete(SourceAction entity) {
    repository.delete(entity);
  }

}
