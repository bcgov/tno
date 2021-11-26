package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Schedule;
import ca.bc.gov.tno.dal.db.repositories.IScheduleRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IScheduleService;

/**
 * RoleService class, provides a concrete way to interact with schedules in the
 * database.
 */
@Service
public class ScheduleService implements IScheduleService {

  private IScheduleRepository repository;

  /**
   * Creates a new instance of a ScheduleService object, initializes with
   * specified parameters.
   * 
   * @param repository The schedule repository.
   */
  @Autowired
  public ScheduleService(final IScheduleRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of schedules.
   */
  @Override
  public List<Schedule> findAll() {
    var schedules = (List<Schedule>) repository.findAll();
    return schedules;
  }

  /**
   * Find the schedule for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the schedule if it exists.
   */
  @Override
  public Optional<Schedule> findById(Integer key) {
    var schedule = repository.findById(key);
    return schedule;
  }

  /**
   * Add a new schedule to the data source.
   * 
   * @param entity The schedule to add.
   * @return A new instance of the schedule that was added.
   */
  @Override
  public Schedule add(Schedule entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified schedule in the data source.
   * 
   * @param entity The schedule to update.
   * @return A new instance of the schedule that was updated.
   */
  @Override
  public Schedule update(Schedule entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified schedule from the data source.
   * 
   * @param entity The schedule to delete.
   */
  @Override
  public void delete(Schedule entity) {
    repository.delete(entity);
  }

}
