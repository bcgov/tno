package ca.bc.gov.tno.dal.db.services;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

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

}
