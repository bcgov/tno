package ca.bc.gov.tno.dal.db;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;

@Component
public class UserTest {
  IUserService userService;

  @Autowired
  public UserTest(final IUserService userService) {
    this.userService = userService;
  }

  public void Run() {
    FindAll();
  }

  public void FindAll() {
    System.out.println("Fetching users");
    var users = userService.findAll();
    users.forEach(u -> System.out.println(u.getUsername()));
  }
}
