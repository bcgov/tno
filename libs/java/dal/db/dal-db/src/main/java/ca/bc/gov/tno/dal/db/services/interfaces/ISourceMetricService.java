package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.SourceMetric;

/**
 * ISourceMetricService interface, provides a way to interact with source
 * metrics.
 */
public interface ISourceMetricService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of sourceSourceMetric.
   */
  List<SourceMetric> findAll();

  /**
   * Find the sourceSourceMetric for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the sourceSourceMetric if it exists.
   */
  Optional<SourceMetric> findById(int key);

  /**
   * Find the sourceSourceMetric for the specified primary key.
   * 
   * @param name The name of the sourceSourceMetric.
   * @return A new instance of the sourceSourceMetric if it exists.
   */
  Optional<SourceMetric> findByName(String name);

  /**
   * Add a new sourceSourceMetric to the content.
   * 
   * @param entity The sourceSourceMetric to add.
   * @return A new instance of the sourceSourceMetric that was added.
   */
  SourceMetric add(SourceMetric entity);

  /**
   * Update the specified sourceSourceMetric in the content.
   * 
   * @param entity The sourceSourceMetric to update.
   * @return A new instance of the sourceSourceMetric that was updated.
   */
  SourceMetric update(SourceMetric entity);

  /**
   * Delete the specified sourceSourceMetric from the content.
   * 
   * @param entity The sourceSourceMetric to delete.
   */
  void delete(SourceMetric entity);
}
