package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.User;

/**
 * IUserService interface, provides a way to interact with users.
 */
public interface IUserService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of users.
   */
  List<User> findAll();

  /**
   * Find the user for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the user if it exists.
   */
  Optional<User> findById(int key);

  /**
   * Add a new user to the data source.
   * 
   * @param entity The user to add.
   * @return A new instance of the user that was added.
   */
  User add(User entity);

  /**
   * Update the specified user in the data source.
   * 
   * @param entity The user to update.
   * @return A new instance of the user that was updated.
   */
  User update(User entity);

  /**
   * Delete the specified user from the data source.
   * 
   * @param entity The user to delete.
   */
  void delete(User entity);
}
