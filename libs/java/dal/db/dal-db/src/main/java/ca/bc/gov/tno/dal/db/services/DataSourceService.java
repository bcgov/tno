package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.dal.db.repositories.IDataSourceRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;

/**
 * DataSourceService class, provides a concrete way to interact with data
 * sources in the database.
 */
@Service
public class DataSourceService implements IDataSourceService {
  private final SessionFactory sessionFactory;
  private final IDataSourceRepository repository;

  /**
   * Creates a new instance of a DataSourceService object, initializes with
   * specified parameters.
   * 
   * @param repository     The data source repository.
   * @param sessionFactory The session factory.
   */
  @Autowired
  public DataSourceService(final IDataSourceRepository repository, final SessionFactory sessionFactory) {
    this.repository = repository;
    this.sessionFactory = sessionFactory;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of data source.
   */
  @Override
  public List<DataSource> findAll() {
    var DataSources = (List<DataSource>) repository.findAll();
    return DataSources;
  }

  /**
   * Find the data source for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the data source if it exists.
   */
  @Override
  public Optional<DataSource> findById(Integer key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Find the data source for the specified code.
   * 
   * @param code The unique code.
   * @return A new instance of the data source if it exists.
   */
  @Override
  public Optional<DataSource> findByCode(String code) {
    var sql = "FROM DataSource ds WHERE ds.code = :code";
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var query = session.createQuery(sql).setParameter("code", code).setMaxResults(1);
      var result = query.uniqueResult();
      return Optional.ofNullable((DataSource) result);
    } finally {
      ts.commit();
    }
  }

  /**
   * Add a new data source to the data source.
   * 
   * @param entity The data source to add.
   * @return A new instance of the data source that was added.
   */
  @Override
  public DataSource add(DataSource entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified data source in the data source.
   * 
   * @param entity The data source to update.
   * @return A new instance of the data source that was updated.
   */
  @Override
  public DataSource update(DataSource entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified data source from the data source.
   * 
   * @param entity The data source to delete.
   */
  @Override
  public void delete(DataSource entity) {
    repository.delete(entity);
  }

}
