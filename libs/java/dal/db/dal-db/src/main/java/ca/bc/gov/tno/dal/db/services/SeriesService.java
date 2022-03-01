package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.Series;
import ca.bc.gov.tno.dal.db.repositories.interfaces.ISeriesRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ISeriesService;

/**
 * SeriesService class, provides a concrete way to interact with series in the
 * database.
 */
@Service
public class SeriesService implements ISeriesService {

  private final ISeriesRepository repository;

  /**
   * Creates a new instance of a SeriesService object, initializes with
   * specified parameters.
   * 
   * @param repository The series repository.
   */
  @Autowired
  public SeriesService(final ISeriesRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of series.
   */
  @Override
  public List<Series> findAll() {
    var result = (List<Series>) repository.findAll();
    return result;
  }

  /**
   * Find the series for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the series if it exists.
   */
  @Override
  public Optional<Series> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Add a new series to the data source.
   * 
   * @param entity The series to add.
   * @return A new instance of the series that was added.
   */
  @Override
  public Series add(Series entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified series in the data source.
   * 
   * @param entity The series to update.
   * @return A new instance of the series that was updated.
   */
  @Override
  public Series update(Series entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified series from the data source.
   * 
   * @param entity The series to delete.
   */
  @Override
  public void delete(Series entity) {
    repository.delete(entity);
  }

}
