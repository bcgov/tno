package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.ContentCategory;
import ca.bc.gov.tno.dal.db.entities.ContentCategoryPK;

/**
 * IContentCategoryService interface, provides a way to interact with content
 * types.
 */
public interface IContentCategoryService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of content category.
   */
  List<ContentCategory> findAll();

  /**
   * Find the content category for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content category if it exists.
   */
  Optional<ContentCategory> findById(ContentCategoryPK key);

  /**
   * Find all content category for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the content category if it exists.
   */
  List<ContentCategory> findById(int contentId);

  /**
   * Add a new content category to the content.
   * 
   * @param entity The content category to add.
   * @return A new instance of the content category that was added.
   */
  ContentCategory add(ContentCategory entity);

  /**
   * Add a new content category to the data source.
   * 
   * @param entities An array of content category to add.
   * @return A new instance of the content category that was added.
   */
  Iterable<ContentCategory> add(Iterable<ContentCategory> entities);

  /**
   * Update the specified content category in the content.
   * 
   * @param entity The content category to update.
   * @return A new instance of the content category that was updated.
   */
  ContentCategory update(ContentCategory entity);

  /**
   * Update the specified content category in the data source.
   * 
   * @param entities An array of content category to update.
   * @return A new instance of the content category that was updated.
   */
  Iterable<ContentCategory> update(Iterable<ContentCategory> entities);

  /**
   * Delete the specified content category from the content.
   * 
   * @param entity The content category to delete.
   */
  void delete(ContentCategory entity);

  /**
   * Delete the specified content category from the data source.
   * 
   * @param entities An array of content category to delete.
   */
  void delete(Iterable<ContentCategory> entities);
}
