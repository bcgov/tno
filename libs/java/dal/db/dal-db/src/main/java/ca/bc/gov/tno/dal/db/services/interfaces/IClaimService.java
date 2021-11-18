package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.Claim;

/**
 * IClaimService interface, provides a way to interact with claims.
 */
public interface IClaimService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of claims.
   */
  List<Claim> findAll();

  /**
   * Find the claim for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the claim if it exists.
   */
  Optional<Claim> findById(Integer key);

  /**
   * Add a new claim to the data source.
   * 
   * @param entity The claim to add.
   * @return A new instance of the claim that was added.
   */
  Claim add(Claim entity);

  /**
   * Update the specified claim in the data source.
   * 
   * @param entity The claim to update.
   * @return A new instance of the claim that was updated.
   */
  Claim update(Claim entity);

  /**
   * Delete the specified claim from the data source.
   * 
   * @param entity The claim to delete.
   */
  void delete(Claim entity);
}
