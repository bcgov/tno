package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.TimeTracking;
import ca.bc.gov.tno.dal.db.entities.TimeTrackingPK;
import ca.bc.gov.tno.dal.db.repositories.ITimeTrackingRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ITimeTrackingService;

/**
 * TimeTrackingService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class TimeTrackingService implements ITimeTrackingService {

  private final SessionFactory sessionFactory;
  private final ITimeTrackingRepository repository;

  /**
   * Creates a new instance of a TimeTrackingService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The time tracking repository.
   */
  @Autowired
  public TimeTrackingService(final SessionFactory sessionFactory, final ITimeTrackingRepository repository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of time tracking.
   */
  @Override
  public List<TimeTracking> findAll() {
    var TimeTrackings = (List<TimeTracking>) repository.findAll();
    return TimeTrackings;
  }

  /**
   * Find the time tracking for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the time tracking if it exists.
   */
  @Override
  public Optional<TimeTracking> findById(TimeTrackingPK key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Find all time tracking for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the time tracking if it exists.
   */
  @Override
  public List<TimeTracking> findByContentId(int contentId) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var results = session
          .createQuery("SELECT tt FROM TimeTracking tt WHERE tt.contentId=:id")
          .setParameter("id", contentId)
          .getResultList();
      return ListHelper.castList(TimeTracking.class, results);
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Add a new time tracking to the data source.
   * 
   * @param entity The time tracking to add.
   * @return A new instance of the time tracking that was added.
   */
  @Override
  public TimeTracking add(TimeTracking entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Add a new time tracking to the data source.
   * 
   * @param entities An array of time tracking to add.
   * @return A new instance of the time tracking that was added.
   */
  @Override
  public Iterable<TimeTracking> add(Iterable<TimeTracking> entities) {
    var result = repository.saveAll(
        StreamSupport.stream(entities.spliterator(), false).map((entity) -> PrincipalHelper.addAudit(entity)).toList());
    return result;
  }

  /**
   * Update the specified time tracking in the data source.
   * 
   * @param entity The time tracking to update.
   * @return A new instance of the time tracking that was updated.
   */
  @Override
  public TimeTracking update(TimeTracking entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Update the specified time tracking in the data source.
   * 
   * @param entities An array of time tracking to update.
   * @return A new instance of the time tracking that was updated.
   */
  @Override
  public Iterable<TimeTracking> update(Iterable<TimeTracking> entities) {
    var result = repository.saveAll(StreamSupport.stream(entities.spliterator(), false)
        .map((entity) -> PrincipalHelper.updateAudit(entity)).toList());
    return result;
  }

  /**
   * Delete the specified time tracking from the data source.
   * 
   * @param entity The time tracking to delete.
   */
  @Override
  public void delete(TimeTracking entity) {
    repository.delete(entity);
  }

  /**
   * Delete the specified time tracking from the data source.
   * 
   * @param entities An array of time tracking to delete.
   */
  @Override
  public void delete(Iterable<TimeTracking> entities) {
    repository.deleteAll(entities);
  }

}
