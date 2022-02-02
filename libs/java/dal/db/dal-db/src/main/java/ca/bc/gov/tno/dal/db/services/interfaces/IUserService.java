package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.User;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.models.interfaces.IPaged;

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
   * Find a page of users that match the query.
   * 
   * @param page     The page to pull users from.
   * @param quantity Number of items to return in a page.
   * @param sort     An array of sort parameters ['col1 desc', 'col2 asc']
   * @return A page of users.
   */
  IPaged<User> find(int page, int quantity, SortParam[] sort);

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
