package ca.bc.gov.tno.dal.db.repositories.interfaces;

import java.util.Optional;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.User;

/**
 * IUserRepository interface, provides a way to interact with the User
 * repository.
 */
@Repository
public interface IUserRepository extends JpaRepository<User, Integer> {

  /**
   * Find the user by it's username.
   * 
   * @param username The unique username to search for.
   * @return An Optional{User} if found with the specified username.
   */
  Optional<User> findByUsername(String username);
}