package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Schedule;

/**
 * IScheduleRepository interface, provides a way to interact with the Schedule
 * repository.
 */
@Repository
public interface IScheduleRepository extends JpaRepository<Schedule, Integer> {

}