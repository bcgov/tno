package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Claim;
import ca.bc.gov.tno.dal.db.repositories.IClaimRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IClaimService;

/**
 * ClaimService class, provides a concrete way to interact with claims in the
 * database.
 */
@Service
public class ClaimService implements IClaimService {

  private final IClaimRepository repository;

  /**
   * Creates a new instance of a ClaimService object, initializes with specified
   * parameters.
   * 
   * @param repository The claim repository.
   */
  @Autowired
  public ClaimService(final IClaimRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of claims.
   */
  @Override
  public List<Claim> findAll() {
    var claims = (List<Claim>) repository.findAll();
    return claims;
  }

  /**
   * Find the claim for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the claim if it exists.
   */
  @Override
  public Optional<Claim> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new claim to the data source.
   * 
   * @param entity The claim to add.
   * @return A new instance of the claim that was added.
   */
  @Override
  public Claim add(Claim entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified claim in the data source.
   * 
   * @param entity The claim to update.
   * @return A new instance of the claim that was updated.
   */
  @Override
  public Claim update(Claim entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified claim from the data source.
   * 
   * @param entity The claim to delete.
   */
  @Override
  public void delete(Claim entity) {
    repository.delete(entity);
  }

}
