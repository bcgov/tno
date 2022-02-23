package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Claim;

/**
 * IClaimRepository interface, provides a way to interact with the Claim
 * repository.
 */
@Repository
public interface IClaimRepository extends JpaRepository<Claim, Integer> {

}