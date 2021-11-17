package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.License;

/**
 * ILicenseService interface, provides a way to interact with licenses.
 */
public interface ILicenseService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of license.
   */
  List<License> findAll();

  /**
   * Find the license for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the license if it exists.
   */
  Optional<License> findById(Integer key);

  /**
   * Add a new license to the data source.
   * 
   * @param entity The license to add.
   * @return A new instance of the license that was added.
   */
  License add(License entity);

  /**
   * Update the specified license in the data source.
   * 
   * @param entity The license to update.
   * @return A new instance of the license that was updated.
   */
  License update(License entity);

  /**
   * Delete the specified license from the data source.
   * 
   * @param entity The license to delete.
   */
  void delete(License entity);
}
