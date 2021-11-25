package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.DataSourceType;
import ca.bc.gov.tno.dal.db.repositories.IDataSourceTypeRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceTypeService;

/**
 * DataSourceTypeService class, provides a concrete way to interact with data
 * source types in the database.
 */
@Service
public class DataSourceTypeService implements IDataSourceTypeService {

  private final IDataSourceTypeRepository repository;

  /**
   * Creates a new instance of a DataSourceTypeService object, initializes with
   * specified parameters.
   * 
   * @param repository The data source type repository.
   */
  @Autowired
  public DataSourceTypeService(final IDataSourceTypeRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of data source type.
   */
  @Override
  public List<DataSourceType> findAll() {
    var dataSourceTypes = (List<DataSourceType>) repository.findAll();
    return dataSourceTypes;
  }

  /**
   * Find the data source type for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the data source type if it exists.
   */
  @Override
  public Optional<DataSourceType> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new data source type to the data source.
   * 
   * @param entity The data source type to add.
   * @return A new instance of the data source type that was added.
   */
  @Override
  public DataSourceType add(DataSourceType entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified data source type in the data source.
   * 
   * @param entity The data source type to update.
   * @return A new instance of the data source type that was updated.
   */
  @Override
  public DataSourceType update(DataSourceType entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified data source type from the data source.
   * 
   * @param entity The data source type to delete.
   */
  @Override
  public void delete(DataSourceType entity) {
    repository.delete(entity);
  }

}
