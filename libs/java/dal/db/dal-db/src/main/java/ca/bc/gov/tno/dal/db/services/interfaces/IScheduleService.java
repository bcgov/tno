package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Schedule;

public interface IScheduleService {
  List<Schedule> findAll();

  Optional<Schedule> findById(Integer key);

  Schedule add(Schedule entity);

  Schedule update(Schedule entity);

  void delete(Schedule entity);
}
