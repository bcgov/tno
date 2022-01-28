package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Tag;

/**
 * ITagService interface, provides a way to interact with content
 * types.
 */
public interface ITagService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of tag.
   */
  List<Tag> findAll();

  /**
   * Find the tag for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the tag if it exists.
   */
  Optional<Tag> findById(String key);

  /**
   * Add a new tag to the content.
   * 
   * @param entity The tag to add.
   * @return A new instance of the tag that was added.
   */
  Tag add(Tag entity);

  /**
   * Update the specified tag in the content.
   * 
   * @param entity The tag to update.
   * @return A new instance of the tag that was updated.
   */
  Tag update(Tag entity);

  /**
   * Delete the specified tag from the content.
   * 
   * @param entity The tag to delete.
   */
  void delete(Tag entity);
}
