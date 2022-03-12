package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.DataLocation;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IDataLocationRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataLocationService;

/**
 * DataLocationService class, provides a concrete way to interact with data
 * locations in the database.
 */
@Service
public class DataLocationService implements IDataLocationService {
  private final SessionFactory sessionFactory;
  private final IDataLocationRepository repository;

  /**
   * Creates a new instance of a DataLocationService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The data location repository.
   */
  @Autowired
  public DataLocationService(final SessionFactory sessionFactory, final IDataLocationRepository repository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of data location.
   */
  @Override
  public List<DataLocation> findAll() {
    var result = (List<DataLocation>) repository.findAll();
    return result;
  }

  /**
   * Find the data location for the specified primary key.
   * 
   * @param key The primary key.
   * @return An instance of the data location if it exists.
   */
  @Override
  public Optional<DataLocation> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find the data location for the specified primary key.
   * 
   * @param name The name of the data location.
   * @return An instance of the data location if it exists.
   */
  @Override
  public Optional<DataLocation> findByName(String name) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var sql = """
          FROM DataLocation mt
          WHERE mt.name = :name
          """;
      var query = session.createQuery(sql).setParameter("name", name).setMaxResults(1);
      return ListHelper.castList(DataLocation.class, query.getResultList()).stream().findFirst();
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Add a new data location to the data source.
   * 
   * @param entity The data location to add.
   * @return An instance of the data location that was added.
   */
  @Override
  public DataLocation add(DataLocation entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified data location in the data source.
   * 
   * @param entity The data location to update.
   * @return An instance of the data location that was updated.
   */
  @Override
  public DataLocation update(DataLocation entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified data location from the data source.
   * 
   * @param entity The data location to delete.
   */
  @Override
  public void delete(DataLocation entity) {
    repository.delete(entity);
  }

}
