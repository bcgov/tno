package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Claim;
import ca.bc.gov.tno.dal.db.repositories.IClaimRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IClaimService;

@Service
public class ClaimService implements IClaimService {

  @Autowired
  private IClaimRepository repository;

  @Override
  public List<Claim> findAll() {
    var claims = (List<Claim>) repository.findAll();
    return claims;
  }

  /**
   * Find the claim for the specified 'key'.
   */
  @Override
  public Optional<Claim> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add the claim.
   */
  @Override
  public Claim add(Claim entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the claim.
   */
  @Override
  public Claim update(Claim entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the claim.
   */
  @Override
  public void delete(Claim entity) {
    repository.delete(entity);
  }

}
