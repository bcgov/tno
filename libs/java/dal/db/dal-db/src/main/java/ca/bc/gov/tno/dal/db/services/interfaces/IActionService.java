package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Action;

/**
 * IActionService interface, provides a way to interact with content
 * types.
 */
public interface IActionService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of action.
   */
  List<Action> findAll();

  /**
   * Find the action for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the action if it exists.
   */
  Optional<Action> findById(int key);

  /**
   * Add a new action to the content.
   * 
   * @param entity The action to add.
   * @return A new instance of the action that was added.
   */
  Action add(Action entity);

  /**
   * Update the specified action in the content.
   * 
   * @param entity The action to update.
   * @return A new instance of the action that was updated.
   */
  Action update(Action entity);

  /**
   * Delete the specified action from the content.
   * 
   * @param entity The action to delete.
   */
  void delete(Action entity);
}
