package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Action;

/**
 * IActionRepository interface, provides a way to interact with the
 * Action repository.
 */
@Repository
public interface IActionRepository extends CrudRepository<Action, Integer> {

}