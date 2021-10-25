package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Schedule;
import ca.bc.gov.tno.dal.db.repositories.IScheduleRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IScheduleService;

@Service
public class ScheduleService implements IScheduleService {

  @Autowired
  private IScheduleRepository repository;

  @Override
  public List<Schedule> findAll() {
    var schedules = (List<Schedule>) repository.findAll();
    return schedules;
  }

  /**
   * Find the schedule for the specified 'key'.
   */
  @Override
  public Optional<Schedule> findById(Integer key) {
    var schedule = repository.findById(key);
    return schedule;
  }

  /**
   * Add the schedule.
   */
  @Override
  public Schedule add(Schedule entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the schedule.
   */
  @Override
  public Schedule update(Schedule entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the schedule.
   */
  @Override
  public void delete(Schedule entity) {
    repository.delete(entity);
  }

}
