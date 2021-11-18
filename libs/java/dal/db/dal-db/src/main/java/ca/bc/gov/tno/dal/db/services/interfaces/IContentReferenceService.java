package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;

/**
 * IContentReferenceService interface, provides a way to interact with content
 * references.
 */
public interface IContentReferenceService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of content reference.
   */
  List<ContentReference> findAll();

  /**
   * Find the content reference for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content reference if it exists.
   */
  Optional<ContentReference> findById(ContentReferencePK key);

  /**
   * Add a new content reference to the data source.
   * 
   * @param entity The content reference to add.
   * @return A new instance of the content reference that was added.
   */
  ContentReference add(ContentReference entity);

  /**
   * Update the specified content reference in the data source.
   * 
   * @param entity The content reference to update.
   * @return A new instance of the content reference that was updated.
   */
  ContentReference update(ContentReference entity);

  /**
   * Delete the specified content reference from the data source.
   * 
   * @param entity The content reference to delete.
   */
  void delete(ContentReference entity);
}
