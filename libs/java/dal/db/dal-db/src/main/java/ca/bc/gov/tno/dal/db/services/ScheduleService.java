package ca.bc.gov.tno.dal.db.services;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

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

}
