package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.ContentType;

/**
 * IContentTypeService interface, provides a way to interact with content
 * types.
 */
public interface IContentTypeService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of content type.
   */
  List<ContentType> findAll();

  /**
   * Find the content type for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content type if it exists.
   */
  Optional<ContentType> findById(int key);

  /**
   * Add a new content type to the content.
   * 
   * @param entity The content type to add.
   * @return A new instance of the content type that was added.
   */
  ContentType add(ContentType entity);

  /**
   * Update the specified content type in the content.
   * 
   * @param entity The content type to update.
   * @return A new instance of the content type that was updated.
   */
  ContentType update(ContentType entity);

  /**
   * Delete the specified content type from the content.
   * 
   * @param entity The content type to delete.
   */
  void delete(ContentType entity);
}
