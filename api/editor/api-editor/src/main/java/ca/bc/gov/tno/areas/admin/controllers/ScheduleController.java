package ca.bc.gov.tno.areas.admin.controllers;

import java.util.List;

import javax.annotation.security.RolesAllowed;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import ca.bc.gov.tno.dal.db.services.interfaces.IScheduleService;
import ca.bc.gov.tno.dal.db.entities.Schedule;

/**
 * Endpoints to communicate with the TNO DB schedules.
 */
@RolesAllowed("administrator")
@RestController("AdminScheduleController")
@RequestMapping({ "/admin/schedules", "/api/admin/schedules" })
public class ScheduleController {

  @Autowired
  private IScheduleService scheduleService;

  /**
   * Request a list of all schedules from the db.
   *
   * @return
   */
  @GetMapping(path = { "", "/" }, produces = MediaType.APPLICATION_JSON_VALUE)
  public List<Schedule> findAll() {
    var schedules = scheduleService.findAll();
    return schedules;
  }

  /**
   * Request a list of all schedules from the db.
   *
   * @param id The primary key.
   * @return
   */
  @GetMapping(path = "/{id}", produces = MediaType.APPLICATION_JSON_VALUE)
  public Schedule findById(@PathVariable(required = true) Integer id) {
    var schedule = scheduleService.findById(id).orElse(null);
    return schedule;
  }

  /**
   * Add a new schedule to the db.
   *
   * @param model
   * @return
   */
  @PostMapping(path = { "", "/" }, consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Schedule add(@RequestBody Schedule model) {
    var schedule = scheduleService.add(model);
    return schedule;
  }

  /**
   * Update the schedule in the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @PutMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Schedule update(@PathVariable Integer id, @RequestBody Schedule model) {
    var schedule = scheduleService.update(model);
    return schedule;
  }

  /**
   * Delete the schedule from the db.
   *
   * @param id    The primary key.
   * @param model
   * @return
   */
  @DeleteMapping(path = "/{id}", consumes = {
      MediaType.APPLICATION_JSON_VALUE }, produces = MediaType.APPLICATION_JSON_VALUE)
  public Schedule delete(@PathVariable Integer id, @RequestBody Schedule model) {
    scheduleService.delete(model);
    return model;
  }

}
