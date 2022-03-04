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
   * Find all content tone for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the content tone if it exists.
   */
  List<ContentTone> findByContentId(int contentId);

  /**
   * Add a new content tone to the content.
   * 
   * @param entity The content tone to add.
   * @return A new instance of the content tone that was added.
   */
  ContentTone add(ContentTone entity);

  /**
   * Add a new content tone pool to the data source.
   * 
   * @param entities An array of content tone pool to add.
   * @return A new instance of the content tone pool that was added.
   */
  Iterable<ContentTone> add(Iterable<ContentTone> entities);

  /**
   * Update the specified content tone in the content.
   * 
   * @param entity The content tone to update.
   * @return A new instance of the content tone that was updated.
   */
  ContentTone update(ContentTone entity);

  /**
   * Update the specified content tone pool in the data source.
   * 
   * @param entities An array of content tone pool to update.
   * @return A new instance of the content tone pool that was updated.
   */
  Iterable<ContentTone> update(Iterable<ContentTone> entities);

  /**
   * Delete the specified content tone from the content.
   * 
   * @param entity The content tone to delete.
   */
  void delete(ContentTone entity);

  /**
   * Delete the specified content tone pool from the data source.
   * 
   * @param entities An array of content tone pool to delete.
   */
  void delete(Iterable<ContentTone> entities);
}
