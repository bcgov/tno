package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.User;
import ca.bc.gov.tno.dal.db.repositories.IUserRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;

@Service
public class UserService implements IUserService {

  @Autowired
  private IUserRepository repository;

  @Override
  public List<User> findAll() {
    var users = (List<User>) repository.findAll();
    return users;
  }

  /**
   * Find the user for the specified 'key'.
   */
  @Override
  public Optional<User> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add the user.
   */
  @Override
  public User add(User entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the user.
   */
  @Override
  public User update(User entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the user.
   */
  @Override
  public void delete(User entity) {
    repository.delete(entity);
  }

}
