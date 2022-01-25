package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.ContentAction;
import ca.bc.gov.tno.dal.db.entities.ContentActionPK;

/**
 * IContentActionService interface, provides a way to interact with content
 * types.
 */
public interface IContentActionService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of content action.
   */
  List<ContentAction> findAll();

  /**
   * Find the content action for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content action if it exists.
   */
  Optional<ContentAction> findById(ContentActionPK key);

  /**
   * Add a new content action to the content.
   * 
   * @param entity The content action to add.
   * @return A new instance of the content action that was added.
   */
  ContentAction add(ContentAction entity);

  /**
   * Update the specified content action in the content.
   * 
   * @param entity The content action to update.
   * @return A new instance of the content action that was updated.
   */
  ContentAction update(ContentAction entity);

  /**
   * Delete the specified content action from the content.
   * 
   * @param entity The content action to delete.
   */
  void delete(ContentAction entity);
}
