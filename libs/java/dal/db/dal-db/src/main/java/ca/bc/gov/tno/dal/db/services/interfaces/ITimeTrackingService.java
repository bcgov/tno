package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.TimeTracking;
import ca.bc.gov.tno.dal.db.entities.TimeTrackingPK;
import ca.bc.gov.tno.dal.db.models.FilterCollection;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.models.interfaces.IPaged;

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
   * Find all time tracking for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the time tracking if it exists.
   */
  List<TimeTracking> findByContentId(int contentId);

  /**
   * Find a page of TimeTracking that match the query.
   * 
   * @param page     The page to pull TimeTracking from.
   * @param quantity Number of items to return in a page.
   * @param filter   An array of filter parameters.
   * @param sort     An array of sort parameters ['col1 desc', 'col2 asc']
   * @return A page of TimeTracking.
   */
  IPaged<TimeTracking> find(int page, int quantity, FilterCollection filter, SortParam[] sort);

  /**
   * Add a new time tracking to the content.
   * 
   * @param entity The time tracking to add.
   * @return A new instance of the time tracking that was added.
   */
  TimeTracking add(TimeTracking entity);

  /**
   * Add a new time tracking to the data source.
   * 
   * @param entities An array of time tracking to add.
   * @return A new instance of the time tracking that was added.
   */
  Iterable<TimeTracking> add(Iterable<TimeTracking> entities);

  /**
   * Update the specified time tracking in the content.
   * 
   * @param entity The time tracking to update.
   * @return A new instance of the time tracking that was updated.
   */
  TimeTracking update(TimeTracking entity);

  /**
   * Update the specified time tracking in the data source.
   * 
   * @param entities An array of time tracking to update.
   * @return A new instance of the time tracking that was updated.
   */
  Iterable<TimeTracking> update(Iterable<TimeTracking> entities);

  /**
   * Delete the specified time tracking from the content.
   * 
   * @param entity The time tracking to delete.
   */
  void delete(TimeTracking entity);

  /**
   * Delete the specified time tracking from the data source.
   * 
   * @param entities An array of time tracking to delete.
   */
  void delete(Iterable<TimeTracking> entities);
}
