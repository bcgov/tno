package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.ContentTag;
import ca.bc.gov.tno.dal.db.entities.ContentTagPK;

/**
 * IContentTagService interface, provides a way to interact with content
 * types.
 */
public interface IContentTagService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of content tag.
   */
  List<ContentTag> findAll();

  /**
   * Find the content tag for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content tag if it exists.
   */
  Optional<ContentTag> findById(ContentTagPK key);

  /**
   * Find all content tag for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the content tag if it exists.
   */
  List<ContentTag> findById(int contentId);

  /**
   * Add a new content tag to the content.
   * 
   * @param entity The content tag to add.
   * @return A new instance of the content tag that was added.
   */
  ContentTag add(ContentTag entity);

  /**
   * Add a new content tag to the data source.
   * 
   * @param entities An array of content tag to add.
   * @return A new instance of the content tag that was added.
   */
  Iterable<ContentTag> add(Iterable<ContentTag> entities);

  /**
   * Update the specified content tag in the content.
   * 
   * @param entity The content tag to update.
   * @return A new instance of the content tag that was updated.
   */
  ContentTag update(ContentTag entity);

  /**
   * Update the specified content tag in the data source.
   * 
   * @param entities An array of content tag to update.
   * @return A new instance of the content tag that was updated.
   */
  Iterable<ContentTag> update(Iterable<ContentTag> entities);

  /**
   * Delete the specified content tag from the content.
   * 
   * @param entity The content tag to delete.
   */
  void delete(ContentTag entity);

  /**
   * Delete the specified content tag from the data source.
   * 
   * @param entities An array of content tag to delete.
   */
  void delete(Iterable<ContentTag> entities);
}
