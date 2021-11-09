package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Claim;

/**
 * IClaimRepository interface, provides a way to interact with the Claim
 * repository.
 */
@Repository
public interface IClaimRepository extends CrudRepository<Claim, Integer> {

}