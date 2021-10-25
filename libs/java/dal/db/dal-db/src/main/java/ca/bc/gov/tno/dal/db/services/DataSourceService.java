package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.dal.db.repositories.IDataSourceRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;

@Service
public class DataSourceService implements IDataSourceService {

  @Autowired
  private IDataSourceRepository repository;

  @Override
  public List<DataSource> findAll() {
    var DataSources = (List<DataSource>) repository.findAll();
    return DataSources;
  }

  /**
   * Find the data source for the specified 'key'.
   */
  @Override
  public Optional<DataSource> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add the data source.
   */
  @Override
  public DataSource add(DataSource entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the data source.
   */
  @Override
  public DataSource update(DataSource entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the data source.
   */
  @Override
  public void delete(DataSource entity) {
    repository.delete(entity);
  }

}
