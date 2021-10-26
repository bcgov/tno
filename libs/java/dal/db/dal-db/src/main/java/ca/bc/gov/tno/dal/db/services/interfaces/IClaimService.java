package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Claim;

public interface IClaimService {
  List<Claim> findAll();

  Optional<Claim> findById(Integer key);

  Claim add(Claim entity);

  Claim update(Claim entity);

  void delete(Claim entity);
}
