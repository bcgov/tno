package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.TonePool;
import ca.bc.gov.tno.dal.db.repositories.interfaces.ITonePoolRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ITonePoolService;

/**
 * TonePoolService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class TonePoolService implements ITonePoolService {

  private final ITonePoolRepository repository;

  /**
   * Creates a new instance of a TonePoolService object, initializes with
   * specified parameters.
   * 
   * @param repository The tone pool repository.
   */
  @Autowired
  public TonePoolService(final ITonePoolRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of tone pool.
   */
  @Override
  public List<TonePool> findAll() {
    var result = (List<TonePool>) repository.findAll();
    return result;
  }

  /**
   * Find the tone pool for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the tone pool if it exists.
   */
  @Override
  public Optional<TonePool> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Add a new tone pool to the data source.
   * 
   * @param entity The tone pool to add.
   * @return A new instance of the tone pool that was added.
   */
  @Override
  public TonePool add(TonePool entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified tone pool in the data source.
   * 
   * @param entity The tone pool to update.
   * @return A new instance of the tone pool that was updated.
   */
  @Override
  public TonePool update(TonePool entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified tone pool from the data source.
   * 
   * @param entity The tone pool to delete.
   */
  @Override
  public void delete(TonePool entity) {
    repository.delete(entity);
  }

}
