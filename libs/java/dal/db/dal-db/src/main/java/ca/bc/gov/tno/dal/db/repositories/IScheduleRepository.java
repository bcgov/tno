package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Schedule;

/**
 * IScheduleRepository interface, provides a way to interact with the Schedule
 * repository.
 */
@Repository
public interface IScheduleRepository extends CrudRepository<Schedule, Integer> {

}