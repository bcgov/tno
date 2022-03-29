package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.SourceAction;

/**
 * ISourceActionService interface, provides a way to interact with source
 * actions.
 */
public interface ISourceActionService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of sourceSourceAction.
   */
  List<SourceAction> findAll();

  /**
   * Find the sourceSourceAction for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the sourceSourceAction if it exists.
   */
  Optional<SourceAction> findById(int key);

  /**
   * Find the sourceSourceAction for the specified primary key.
   * 
   * @param name The name of the sourceSourceAction.
   * @return A new instance of the sourceSourceAction if it exists.
   */
  Optional<SourceAction> findByName(String name);

  /**
   * Add a new sourceSourceAction to the content.
   * 
   * @param entity The sourceSourceAction to add.
   * @return A new instance of the sourceSourceAction that was added.
   */
  SourceAction add(SourceAction entity);

  /**
   * Update the specified sourceSourceAction in the content.
   * 
   * @param entity The sourceSourceAction to update.
   * @return A new instance of the sourceSourceAction that was updated.
   */
  SourceAction update(SourceAction entity);

  /**
   * Delete the specified sourceSourceAction from the content.
   * 
   * @param entity The sourceSourceAction to delete.
   */
  void delete(SourceAction entity);
}
