package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;

import ca.bc.gov.tno.dal.db.entities.User;

public interface IUserService {
  List<User> findAll();
}
