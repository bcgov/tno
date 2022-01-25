package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.ContentTone;
import ca.bc.gov.tno.dal.db.entities.ContentTonePK;

/**
 * IContentToneService interface, provides a way to interact with content
 * types.
 */
public interface IContentToneService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of content tone.
   */
  List<ContentTone> findAll();

  /**
   * Find the content tone for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content tone if it exists.
   */
  Optional<ContentTone> findById(ContentTonePK key);

  /**
   * Add a new content tone to the content.
   * 
   * @param entity The content tone to add.
   * @return A new instance of the content tone that was added.
   */
  ContentTone add(ContentTone entity);

  /**
   * Update the specified content tone in the content.
   * 
   * @param entity The content tone to update.
   * @return A new instance of the content tone that was updated.
   */
  ContentTone update(ContentTone entity);

  /**
   * Delete the specified content tone from the content.
   * 
   * @param entity The content tone to delete.
   */
  void delete(ContentTone entity);
}
