package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.User;
import ca.bc.gov.tno.dal.db.repositories.IUserRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;

/**
 * RoleService class, provides a concrete way to interact with users in the
 * database.
 */
@Service
public class UserService implements IUserService {

  private IUserRepository repository;

  /**
   * Creates a new instance of a UserService object, initializes with specified
   * parameters.
   * 
   * @param repository The user repository.
   */
  @Autowired
  public UserService(final IUserRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of users.
   */
  @Override
  public List<User> findAll() {
    var users = (List<User>) repository.findAll();
    return users;
  }

  /**
   * Find the user for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the user if it exists.
   */
  @Override
  public Optional<User> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new user to the data source.
   * 
   * @param entity The user to add.
   * @return A new instance of the user that was added.
   */
  @Override
  public User add(User entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified user in the data source.
   * 
   * @param entity The user to update.
   * @return A new instance of the user that was updated.
   */
  @Override
  public User update(User entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified user from the data source.
   * 
   * @param entity The user to delete.
   */
  @Override
  public void delete(User entity) {
    repository.delete(entity);
  }

}
