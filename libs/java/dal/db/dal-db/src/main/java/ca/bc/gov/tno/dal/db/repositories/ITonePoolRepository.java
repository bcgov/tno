package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.TonePool;

/**
 * ITonePoolRepository interface, provides a way to interact with the
 * TonePool repository.
 */
@Repository
public interface ITonePoolRepository extends CrudRepository<TonePool, Integer> {

}