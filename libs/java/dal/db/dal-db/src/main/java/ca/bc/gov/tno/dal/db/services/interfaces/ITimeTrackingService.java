package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.TimeTracking;
import ca.bc.gov.tno.dal.db.entities.TimeTrackingPK;

/**
 * ITimeTrackingService interface, provides a way to interact with content
 * types.
 */
public interface ITimeTrackingService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of time tracking.
   */
  List<TimeTracking> findAll();

  /**
   * Find the time tracking for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the time tracking if it exists.
   */
  Optional<TimeTracking> findById(TimeTrackingPK key);

  /**
   * Add a new time tracking to the content.
   * 
   * @param entity The time tracking to add.
   * @return A new instance of the time tracking that was added.
   */
  TimeTracking add(TimeTracking entity);

  /**
   * Update the specified time tracking in the content.
   * 
   * @param entity The time tracking to update.
   * @return A new instance of the time tracking that was updated.
   */
  TimeTracking update(TimeTracking entity);

  /**
   * Delete the specified time tracking from the content.
   * 
   * @param entity The time tracking to delete.
   */
  void delete(TimeTracking entity);
}
