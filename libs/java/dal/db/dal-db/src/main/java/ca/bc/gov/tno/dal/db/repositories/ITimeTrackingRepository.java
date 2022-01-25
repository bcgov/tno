package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.TimeTracking;
import ca.bc.gov.tno.dal.db.entities.TimeTrackingPK;

/**
 * ITimeTrackingRepository interface, provides a way to interact with the
 * TimeTracking repository.
 */
@Repository
public interface ITimeTrackingRepository extends CrudRepository<TimeTracking, TimeTrackingPK> {

}