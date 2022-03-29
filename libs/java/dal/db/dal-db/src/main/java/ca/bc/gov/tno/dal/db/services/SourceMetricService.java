package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.SourceMetric;
import ca.bc.gov.tno.dal.db.repositories.interfaces.ISourceMetricRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ISourceMetricService;

/**
 * SourceMetricService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class SourceMetricService implements ISourceMetricService {

  private final ISourceMetricRepository repository;

  /**
   * Creates a new instance of a SourceMetricService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The source metric repository.
   */
  @Autowired
  public SourceMetricService(final SessionFactory sessionFactory, final ISourceMetricRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of sourceMetric.
   */
  @Override
  public List<SourceMetric> findAll() {
    var result = (List<SourceMetric>) repository.findAll();
    return result;
  }

  /**
   * Find the sourceMetric for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the sourceMetric if it exists.
   */
  @Override
  public Optional<SourceMetric> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find the sourceMetric for the specified primary key.
   * 
   * @param name The name of the sourceMetric.
   * @return A new instance of the sourceMetric if it exists.
   */
  @Override
  public Optional<SourceMetric> findByName(String name) {
    var result = repository.findByName(name);
    return result;
  }

  /**
   * Add a new sourceMetric to the data source.
   * 
   * @param entity The sourceMetric to add.
   * @return A new instance of the sourceMetric that was added.
   */
  @Override
  public SourceMetric add(SourceMetric entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified sourceMetric in the data source.
   * 
   * @param entity The sourceMetric to update.
   * @return A new instance of the sourceMetric that was updated.
   */
  @Override
  public SourceMetric update(SourceMetric entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified sourceMetric from the data source.
   * 
   * @param entity The sourceMetric to delete.
   */
  @Override
  public void delete(SourceMetric entity) {
    repository.delete(entity);
  }

}
