package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.User;

/**
 * IUserRepository interface, provides a way to interact with the User
 * repository.
 */
@Repository
public interface IUserRepository extends JpaRepository<User, Integer> {

}