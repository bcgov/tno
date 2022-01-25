package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Schedule;

/**
 * IScheduleService interface, provides a way to interact with schedules.
 */
public interface IScheduleService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of schedules.
   */
  List<Schedule> findAll();

  /**
   * Find the schedule for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the schedule if it exists.
   */
  Optional<Schedule> findById(int key);

  /**
   * Add a new schedule to the data source.
   * 
   * @param entity The schedule to add.
   * @return A new instance of the schedule that was added.
   */
  Schedule add(Schedule entity);

  /**
   * Update the specified schedule in the data source.
   * 
   * @param entity The schedule to update.
   * @return A new instance of the schedule that was updated.
   */
  Schedule update(Schedule entity);

  /**
   * Delete the specified schedule from the data source.
   * 
   * @param entity The schedule to delete.
   */
  void delete(Schedule entity);
}
