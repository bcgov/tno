package ca.bc.gov.tno.dal.db.repositories.interfaces;

import java.util.Optional;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.SourceAction;

/**
 * IActionRepository interface, provides a way to interact with the
 * SourceAction repository.
 */
@Repository
public interface ISourceActionRepository extends JpaRepository<SourceAction, Integer> {
  /**
   * Find the source action by it's name.
   * 
   * @param name The name to search for.
   * @return An Optional{SourceAction} if found with the specified name.
   */
  Optional<SourceAction> findByName(String name);
}