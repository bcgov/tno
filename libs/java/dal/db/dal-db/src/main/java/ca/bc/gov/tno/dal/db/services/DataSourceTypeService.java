package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.DataSourceType;
import ca.bc.gov.tno.dal.db.repositories.IDataSourceTypeRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceTypeService;

@Service
public class DataSourceTypeService implements IDataSourceTypeService {

  @Autowired
  private IDataSourceTypeRepository repository;

  @Override
  public List<DataSourceType> findAll() {
    var dataSourceTypes = (List<DataSourceType>) repository.findAll();
    return dataSourceTypes;
  }

  /**
   * Find the data source type for the specified 'key'.
   */
  @Override
  public Optional<DataSourceType> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add the data source type.
   */
  @Override
  public DataSourceType add(DataSourceType entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the data source type.
   */
  @Override
  public DataSourceType update(DataSourceType entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the data source type.
   */
  @Override
  public void delete(DataSourceType entity) {
    repository.delete(entity);
  }

}
