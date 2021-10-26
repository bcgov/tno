package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Role;

public interface IRoleService {
  List<Role> findAll();

  Optional<Role> findById(Integer key);

  Role add(Role entity);

  Role update(Role entity);

  void delete(Role entity);
}
