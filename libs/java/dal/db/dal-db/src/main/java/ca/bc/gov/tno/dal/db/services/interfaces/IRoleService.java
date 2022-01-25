package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Role;

/**
 * IRoleService interface, provides a way to interact with roles.
 */
public interface IRoleService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of roles.
   */
  List<Role> findAll();

  /**
   * Find the role for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the role if it exists.
   */
  Optional<Role> findById(int key);

  /**
   * Add a new role to the data source.
   * 
   * @param entity The role to add.
   * @return A new instance of the role that was added.
   */
  Role add(Role entity);

  /**
   * Update the specified role in the data source.
   * 
   * @param entity The role to update.
   * @return A new instance of the role that was updated.
   */
  Role update(Role entity);

  /**
   * Delete the specified role from the data source.
   * 
   * @param entity The role to delete.
   */
  void delete(Role entity);
}
