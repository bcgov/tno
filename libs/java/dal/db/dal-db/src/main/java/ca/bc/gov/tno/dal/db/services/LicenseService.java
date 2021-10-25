package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.License;
import ca.bc.gov.tno.dal.db.repositories.ILicenseRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ILicenseService;

@Service
public class LicenseService implements ILicenseService {

  @Autowired
  private ILicenseRepository repository;

  @Override
  public List<License> findAll() {
    var licenses = (List<License>) repository.findAll();
    return licenses;
  }

  /**
   * Find the license for the specified 'key'.
   */
  @Override
  public Optional<License> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add the license.
   */
  @Override
  public License add(License entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the license.
   */
  @Override
  public License update(License entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the license.
   */
  @Override
  public void delete(License entity) {
    repository.delete(entity);
  }

}
