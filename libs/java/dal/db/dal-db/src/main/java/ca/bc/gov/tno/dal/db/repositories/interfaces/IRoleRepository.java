package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Role;

/**
 * IRoleRepository interface, provides a way to interact with the Role
 * repository.
 */
@Repository
public interface IRoleRepository extends JpaRepository<Role, Integer> {

}