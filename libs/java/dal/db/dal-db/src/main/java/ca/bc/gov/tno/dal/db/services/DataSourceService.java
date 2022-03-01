package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.dal.db.entities.DataSourceSchedule;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IDataSourceRepository;
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
   * @param sessionFactory The session factory.
   * @param repository     The data source repository.
   */
  @Autowired
  public DataSourceService(final SessionFactory sessionFactory,
      final IDataSourceRepository repository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of data source.
   */
  @Override
  public List<DataSource> findAll() {
    var result = (List<DataSource>) repository.findAll();
    return result;
  }

  /**
   * Find the data source for the specified media type.
   * 
   * @param mediaTypeId Foreign key to media type.
   * @return A new instance of the data source if it exists.
   */
  @Override
  public List<DataSource> findByMediaTypeId(int mediaTypeId) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var sql = """
          FROM DataSource ds
          JOIN FETCH ds.mediaType mt
          JOIN FETCH ds.license l
          LEFT JOIN FETCH ds.parent p
          LEFT JOIN FETCH ds.dataSourceSchedules dss
          LEFT JOIN FETCH dss.schedule
          WHERE ds.mediaTypeId = :mediaTypeId
          """;
      var query = session.createQuery(sql).setParameter("mediaTypeId", mediaTypeId).setMaxResults(1);
      return ListHelper.castList(DataSource.class, query.list());
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Find the data source for the specified primary key.
   * 
   * @param key The primary key.
   * @return An instance of the data source if it exists.
   */
  @Override
  public Optional<DataSource> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find the data source for the specified code.
   * 
   * @param code The unique code.
   * @return An instance of the data source if it exists.
   */
  @Override
  public Optional<DataSource> findByCode(String code) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var sql = """
          FROM DataSource ds
          JOIN FETCH ds.mediaType mt
          JOIN FETCH ds.license l
          LEFT JOIN FETCH ds.parent p
          LEFT JOIN FETCH ds.dataSourceSchedules dss
          LEFT JOIN FETCH dss.schedule
          WHERE ds.code = :code
          """;
      var query = session.createQuery(sql).setParameter("code", code).setMaxResults(1);
      var result = query.uniqueResult();
      return Optional.ofNullable((DataSource) result);
    } finally {
      ts.commit();
      session.close();
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
    for (DataSourceSchedule dss : entity.getDataSourceSchedules()) {
      PrincipalHelper.addAudit(dss);
    }
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
    for (DataSourceSchedule dss : entity.getDataSourceSchedules()) {
      PrincipalHelper.addAudit(dss);
    }
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
