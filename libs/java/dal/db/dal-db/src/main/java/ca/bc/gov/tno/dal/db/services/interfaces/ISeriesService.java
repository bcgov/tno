package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Series;

/**
 * ISeriesService interface, provides a way to interact with series.
 */
public interface ISeriesService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of series.
   */
  List<Series> findAll();

  /**
   * Find the series for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the series if it exists.
   */
  Optional<Series> findById(int key);

  /**
   * Add a new series to the media.
   * 
   * @param entity The series to add.
   * @return A new instance of the series that was added.
   */
  Series add(Series entity);

  /**
   * Update the specified series in the media.
   * 
   * @param entity The series to update.
   * @return A new instance of the series that was updated.
   */
  Series update(Series entity);

  /**
   * Delete the specified series from the media.
   * 
   * @param entity The series to delete.
   */
  void delete(Series entity);
}
