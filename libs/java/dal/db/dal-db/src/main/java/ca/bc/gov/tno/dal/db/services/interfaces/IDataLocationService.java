package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.DataLocation;

/**
 * IDataLocationService interface, provides a way to interact with data
 * locations.
 */
public interface IDataLocationService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of data location.
   */
  List<DataLocation> findAll();

  /**
   * Find the data location for the specified primary key.
   * 
   * @param key The primary key.
   * @return An instance of the data location if it exists.
   */
  Optional<DataLocation> findById(int key);

  /**
   * Find the data location for the specified primary key.
   * 
   * @param name The name of the data location.
   * @return An instance of the data location if it exists.
   */
  Optional<DataLocation> findByName(String name);

  /**
   * Add a new data location to the media.
   * 
   * @param entity The data location to add.
   * @return An instance of the data location that was added.
   */
  DataLocation add(DataLocation entity);

  /**
   * Update the specified data location in the media.
   * 
   * @param entity The data location to update.
   * @return An instance of the data location that was updated.
   */
  DataLocation update(DataLocation entity);

  /**
   * Delete the specified data location from the media.
   * 
   * @param entity The data location to delete.
   */
  void delete(DataLocation entity);
}
