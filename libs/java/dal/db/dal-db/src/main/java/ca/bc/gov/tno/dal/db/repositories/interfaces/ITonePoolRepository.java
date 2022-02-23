package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.TonePool;

/**
 * ITonePoolRepository interface, provides a way to interact with the
 * TonePool repository.
 */
@Repository
public interface ITonePoolRepository extends JpaRepository<TonePool, Integer> {

}