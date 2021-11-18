package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Role;

/**
 * IRoleRepository interface, provides a way to interact with the Role
 * repository.
 */
@Repository
public interface IRoleRepository extends CrudRepository<Role, Integer> {

}