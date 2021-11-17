package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.User;

/**
 * IUserRepository interface, provides a way to interact with the User
 * repository.
 */
@Repository
public interface IUserRepository extends CrudRepository<User, Integer> {

}