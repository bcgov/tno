package ca.bc.gov.tno.dal.db.repositories.interfaces;

import java.util.Optional;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Action;

/**
 * IActionRepository interface, provides a way to interact with the
 * Action repository.
 */
@Repository
public interface IActionRepository extends JpaRepository<Action, Integer> {
  /**
   * Find the action by it's name.
   * 
   * @param name The name to search for.
   * @return An Optional{Action} if found with the specified name.
   */
  Optional<Action> findByName(String name);
}