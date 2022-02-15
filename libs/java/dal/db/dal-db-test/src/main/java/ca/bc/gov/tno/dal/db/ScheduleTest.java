package ca.bc.gov.tno.dal.db;

import java.time.ZonedDateTime;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.entities.Schedule;
import ca.bc.gov.tno.dal.db.services.interfaces.IScheduleService;

@Component
public class ScheduleTest {
  IScheduleService scheduleService;

  @Autowired
  public ScheduleTest(final IScheduleService scheduleService) {
    this.scheduleService = scheduleService;
  }

  public void Run() {
    var result = Add();
    FindById(result);
    Delete(result);
  }

  public Schedule Add() {
    System.out.println("Fetching schedules");
    var sources = scheduleService.findAll();
    sources.forEach(ds -> System.out.println(ds.getName()));

    var find = scheduleService.findById(1);
    if (find.isEmpty())
      throw new IllegalStateException("Should have found a schedule");

    var add = new Schedule();
    add.setRunOn(ZonedDateTime.now());
    add.setName("30 Seconds");
    add.setDelayMS(30000);
    var result = scheduleService.add(add);
    if (result.getId() == 0)
      throw new IllegalStateException();

    return result;
  }

  public Optional<Schedule> FindById(Schedule entity) {

    var result = scheduleService.findById(entity.getId());
    if (result.isEmpty())
      throw new IllegalStateException();

    return result;
  }

  public void Delete(Schedule entity) {
    scheduleService.delete(entity);
  }
}
