package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.User;

public interface IUserService {
  List<User> findAll();

  Optional<User> findById(Integer key);

  User add(User entity);

  User update(User entity);

  void delete(User entity);
}
