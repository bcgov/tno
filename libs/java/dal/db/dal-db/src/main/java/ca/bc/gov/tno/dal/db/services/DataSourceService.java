package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.hibernate.Hibernate;
import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.dal.db.entities.DataSourceAction;
import ca.bc.gov.tno.dal.db.entities.DataSourceMetric;
import ca.bc.gov.tno.dal.db.entities.DataSourceSchedule;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IDataSourceRepository;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IScheduleRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;

/**
 * DataSourceService class, provides a concrete way to interact with data
 * sources in the database.
 */
@Service
public class DataSourceService implements IDataSourceService {
  private final SessionFactory sessionFactory;
  private final IDataSourceRepository repository;
  private final IScheduleRepository scheduleRepository;

  /**
   * Creates a new instance of a DataSourceService object, initializes with
   * specified parameters.
   *
   * @param sessionFactory     The session factory.
   * @param repository         The data source repository.
   * @param scheduleRepository The schedule repository.
   */
  @Autowired
  public DataSourceService(
      final SessionFactory sessionFactory,
      final IDataSourceRepository repository,
      final IScheduleRepository scheduleRepository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
    this.scheduleRepository = scheduleRepository;
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
   * Find the data source for the specified primary key.
   *
   * @param key The primary key.
   * @return An instance of the data source if it exists.
   */
  @Override
  @SuppressWarnings("unchecked")
  public Optional<DataSource> findById(int key) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var sql = """
          SELECT DISTINCT ds
          FROM DataSource ds
          JOIN FETCH ds.mediaType mt
          JOIN FETCH ds.dataLocation dl
          JOIN FETCH ds.license l
          LEFT JOIN FETCH ds.parent p
          LEFT JOIN FETCH ds.dataSourceSchedules dss
          LEFT JOIN FETCH dss.schedule
          WHERE ds.id = :id
          """;
      var query = session.createQuery(sql).setParameter("id", key);
      var result = query.uniqueResultOptional();

      if (!result.isPresent())
        return result;

      var entity = (DataSource) result.get();
      Hibernate.initialize(entity.getDataSourceActions());
      Hibernate.initialize(entity.getDataSourceMetrics());
      entity.getDataSourceActions();
      entity.getDataSourceMetrics();

      return result;
    } finally {
      ts.commit();
      session.close();
    }
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
          JOIN FETCH ds.dataLocation dl
          JOIN FETCH ds.license l
          LEFT JOIN FETCH ds.parent p
          LEFT JOIN FETCH ds.dataSourceSchedules dss
          LEFT JOIN FETCH dss.schedule
          WHERE ds.mediaTypeId = :mediaTypeId
          """;
      var query = session.createQuery(sql).setParameter("mediaTypeId", mediaTypeId);
      return ListHelper.castList(DataSource.class, query.list());
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Find the data source for the specified media type and data location.
   *
   * @param mediaTypeId    Foreign key to media type.
   * @param dataLocationId Foreign key to data location.
   * @return A new instance of the data source if it exists.
   */
  @Override
  public List<DataSource> findByMediaTypeIdAndDataLocationId(int mediaTypeId, Integer dataLocationId) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var sql = """
          FROM DataSource ds
          JOIN FETCH ds.mediaType mt
          JOIN FETCH ds.dataLocation dl
          JOIN FETCH ds.license l
          LEFT JOIN FETCH ds.parent p
          LEFT JOIN FETCH ds.dataSourceSchedules dss
          LEFT JOIN FETCH dss.schedule
          WHERE ds.mediaTypeId = :mediaTypeId
            AND (ds.dataLocationId = :dataLocationId OR null = :dataLocationId)
          """;
      var query = session
          .createQuery(sql)
          .setParameter("mediaTypeId", mediaTypeId)
          .setParameter("dataLocationId", dataLocationId);
      return ListHelper.castList(DataSource.class, query.list());
    } finally {
      ts.commit();
      session.close();
    }
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
          JOIN FETCH ds.dataLocation dl
          JOIN FETCH ds.license l
          LEFT JOIN FETCH ds.parent p
          LEFT JOIN FETCH ds.dataSourceSchedules dss
          LEFT JOIN FETCH dss.schedule
          WHERE ds.code = :code
          """;
      var query = session.createQuery(sql).setParameter("code", code);
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
      var schedule = scheduleRepository.save(PrincipalHelper.addAudit(dss.getSchedule()));
      dss.setScheduleId(schedule.getId());
    }
    for (DataSourceAction dsa : entity.getDataSourceActions()) {
      PrincipalHelper.addAudit(dsa);
    }
    for (DataSourceMetric dsm : entity.getDataSourceMetrics()) {
      PrincipalHelper.addAudit(dsm);
    }
    var result = repository.save(PrincipalHelper.addAudit(entity));
    Hibernate.initialize(result.getDataSourceActions());
    Hibernate.initialize(result.getDataSourceMetrics());
    Hibernate.initialize(result.getDataSourceSchedules());
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
      var schedule = dss.getSchedule();
      if (schedule.getId() == 0)
        PrincipalHelper.addAudit(schedule);
      else
        PrincipalHelper.updateAudit(schedule);
      schedule = scheduleRepository.save(schedule);
      dss.setScheduleId(schedule.getId());
    }
    for (DataSourceAction dsa : entity.getDataSourceActions()) {
      PrincipalHelper.addAudit(dsa);
    }
    for (DataSourceMetric dsm : entity.getDataSourceMetrics()) {
      PrincipalHelper.addAudit(dsm);
    }
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    Hibernate.initialize(result.getDataSourceActions());
    Hibernate.initialize(result.getDataSourceMetrics());
    Hibernate.initialize(result.getDataSourceSchedules());
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
