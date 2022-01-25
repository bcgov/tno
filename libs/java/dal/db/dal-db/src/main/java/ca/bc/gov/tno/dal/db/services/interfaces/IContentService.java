package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Content;

/**
 * IContentService interface, provides a way to interact with contents.
 */
public interface IContentService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of content.
   */
  List<Content> findAll();

  /**
   * Find the content for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content if it exists.
   */
  Optional<Content> findById(int key);

  /**
   * Add a new content to the data source.
   * 
   * @param entity The content to add.
   * @return A new instance of the content that was added.
   */
  Content add(Content entity);

  /**
   * Update the specified content in the data source.
   * 
   * @param entity The content to update.
   * @return A new instance of the content that was updated.
   */
  Content update(Content entity);

  /**
   * Delete the specified content from the data source.
   * 
   * @param entity The content to delete.
   */
  void delete(Content entity);
}
