package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Action;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IActionRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IActionService;

/**
 * ActionService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class ActionService implements IActionService {

  private final IActionRepository repository;

  /**
   * Creates a new instance of a ActionService object, initializes with
   * specified parameters.
   * 
   * @param repository The action repository.
   */
  @Autowired
  public ActionService(final SessionFactory sessionFactory, final IActionRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of action.
   */
  @Override
  public List<Action> findAll() {
    var result = (List<Action>) repository.findAll();
    return result;
  }

  /**
   * Find the action for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the action if it exists.
   */
  @Override
  public Optional<Action> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find the action for the specified primary key.
   * 
   * @param name The name of the action.
   * @return A new instance of the action if it exists.
   */
  @Override
  public Optional<Action> findByName(String name) {
    var result = repository.findByName(name);
    return result;
  }

  /**
   * Add a new action to the data source.
   * 
   * @param entity The action to add.
   * @return A new instance of the action that was added.
   */
  @Override
  public Action add(Action entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified action in the data source.
   * 
   * @param entity The action to update.
   * @return A new instance of the action that was updated.
   */
  @Override
  public Action update(Action entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified action from the data source.
   * 
   * @param entity The action to delete.
   */
  @Override
  public void delete(Action entity) {
    repository.delete(entity);
  }

}
