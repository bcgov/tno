package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.TonePool;

/**
 * ITonePoolService interface, provides a way to interact with content
 * types.
 */
public interface ITonePoolService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of tone pool.
   */
  List<TonePool> findAll();

  /**
   * Find the tone pool for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the tone pool if it exists.
   */
  Optional<TonePool> findById(int key);

  /**
   * Add a new tone pool to the content.
   * 
   * @param entity The tone pool to add.
   * @return A new instance of the tone pool that was added.
   */
  TonePool add(TonePool entity);

  /**
   * Update the specified tone pool in the content.
   * 
   * @param entity The tone pool to update.
   * @return A new instance of the tone pool that was updated.
   */
  TonePool update(TonePool entity);

  /**
   * Delete the specified tone pool from the content.
   * 
   * @param entity The tone pool to delete.
   */
  void delete(TonePool entity);
}
