package ca.bc.gov.tno.dal.services;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.dal.entities.User;
import ca.bc.gov.tno.dal.repository.IUserRepository;

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
