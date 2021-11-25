package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Role;
import ca.bc.gov.tno.dal.db.repositories.IRoleRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IRoleService;

/**
 * RoleService class, provides a concrete way to interact with roles in the
 * database.
 */
@Service
public class RoleService implements IRoleService {

  private IRoleRepository repository;

  /**
   * Creates a new instance of a RoleService object, initializes with specified
   * parameters.
   * 
   * @param repository The role repository.
   */
  @Autowired
  public RoleService(final IRoleRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of roles.
   */
  @Override
  public List<Role> findAll() {
    var roles = (List<Role>) repository.findAll();
    return roles;
  }

  /**
   * Find the role for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the role if it exists.
   */
  @Override
  public Optional<Role> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new role to the data source.
   * 
   * @param entity The role to add.
   * @return A new instance of the role that was added.
   */
  @Override
  public Role add(Role entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified role in the data source.
   * 
   * @param entity The role to update.
   * @return A new instance of the role that was updated.
   */
  @Override
  public Role update(Role entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified role from the data source.
   * 
   * @param entity The role to delete.
   */
  @Override
  public void delete(Role entity) {
    repository.delete(entity);
  }

}
