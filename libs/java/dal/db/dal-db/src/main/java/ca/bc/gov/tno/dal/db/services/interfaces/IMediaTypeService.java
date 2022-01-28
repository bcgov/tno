package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.MediaType;

/**
 * IMediaTypeService interface, provides a way to interact with media
 * types.
 */
public interface IMediaTypeService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of media type.
   */
  List<MediaType> findAll();

  /**
   * Find the media type for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the media type if it exists.
   */
  Optional<MediaType> findById(int key);

  /**
   * Add a new media type to the media.
   * 
   * @param entity The media type to add.
   * @return A new instance of the media type that was added.
   */
  MediaType add(MediaType entity);

  /**
   * Update the specified media type in the media.
   * 
   * @param entity The media type to update.
   * @return A new instance of the media type that was updated.
   */
  MediaType update(MediaType entity);

  /**
   * Delete the specified media type from the media.
   * 
   * @param entity The media type to delete.
   */
  void delete(MediaType entity);
}
