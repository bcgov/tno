package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.License;
import ca.bc.gov.tno.dal.db.repositories.ILicenseRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ILicenseService;

/**
 * DataSourceTypeService class, provides a concrete way to interact with
 * licenses in the database.
 */
@Service
public class LicenseService implements ILicenseService {

  private ILicenseRepository repository;

  /**
   * Creates a new instance of a LicenseService object, initializes with specified
   * parameters.
   * 
   * @param repository The license repository.
   */
  @Autowired
  public LicenseService(final ILicenseRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of license.
   */
  @Override
  public List<License> findAll() {
    var licenses = (List<License>) repository.findAll();
    return licenses;
  }

  /**
   * Find the license for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the license if it exists.
   */
  @Override
  public Optional<License> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new license to the data source.
   * 
   * @param entity The license to add.
   * @return A new instance of the license that was added.
   */
  @Override
  public License add(License entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified license in the data source.
   * 
   * @param entity The license to update.
   * @return A new instance of the license that was updated.
   */
  @Override
  public License update(License entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified license from the data source.
   * 
   * @param entity The license to delete.
   */
  @Override
  public void delete(License entity) {
    repository.delete(entity);
  }

}
