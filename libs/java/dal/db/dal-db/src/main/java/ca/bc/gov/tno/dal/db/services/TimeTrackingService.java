package ca.bc.gov.tno.dal.db.services;

import java.util.Arrays;
import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.SortDirection;
import ca.bc.gov.tno.dal.db.entities.TimeTracking;
import ca.bc.gov.tno.dal.db.entities.TimeTrackingPK;
import ca.bc.gov.tno.dal.db.models.FilterCollection;
import ca.bc.gov.tno.dal.db.models.FilterParam;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.dal.db.repositories.interfaces.ITimeTrackingRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ITimeTrackingService;
import ca.bc.gov.tno.models.Paged;
import ca.bc.gov.tno.models.interfaces.IPaged;

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
    var result = (List<TimeTracking>) repository.findAll();
    return result;
  }

  /**
   * Find the time tracking for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the time tracking if it exists.
   */
  @Override
  public Optional<TimeTracking> findById(TimeTrackingPK key) {
    var result = repository.findById(key);
    return result;
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
   * Find a page of TimeTracking that match the query.
   * 
   * @param page     The page to pull TimeTracking from.
   * @param quantity Number of items to return in a page.
   * @param filter   An array of filter parameters.
   * @param sort     An array of sort parameters ['col1 desc', 'col2 asc']
   * @return A page of TimeTracking.
   */
  public IPaged<TimeTracking> find(int page, int quantity, FilterCollection filter, SortParam[] sort) {
    page = page < 1 ? 1 : page;
    quantity = quantity < 1 ? 10 : quantity;

    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var pageQuery = session
          .createQuery(generateFindHsql("DISTINCT timeTracking", filter, sort, true))
          .setFirstResult((page - 1) * quantity)
          .setMaxResults(quantity);
      var totalQuery = session.createQuery(generateFindHsql("COUNT(*)", filter, sort, false));

      var items = pageQuery.getResultList();
      var total = (long) totalQuery.uniqueResult();

      return new Paged<TimeTracking>(ListHelper.castList(TimeTracking.class, items), page, quantity, total);
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Generate an HSQL join statements for the TimeTracking find.
   * 
   * @param select  What you want to select from TimeTracking. 'SELECT {select}
   *                FROM
   *                TimeTracking'.
   * @param filter  FilterCollection object.
   * @param sort    An array of sort params.
   * @param doFetch Whether to include the fetch statement. Only fetch when
   *                returning TimeTracking.
   * @return HSQL join statement string.
   */
  private String generateFindHsql(String select, FilterCollection filter, SortParam[] sort, boolean doFetch) {

    // TODO: Switch to parameters.
    StringBuilder where = new StringBuilder();
    if (filter != null && filter.getFilters().size() > 0) {
      where.append("WHERE");
      var filters = filter.getFilters();
      var first = true;
      for (Object op : filters) {
        var param = (FilterParam<?>) op;

        where.append(String.format("%s %s\n", (!first ? " AND" : ""), param.toString("timeTracking")));
        first = false;
      }
    }

    // Default sorting.
    if (sort == null || sort.length == 0)
      sort = new SortParam[] {
          new SortParam("timeTracking", "userId", SortDirection.Ascending),
          new SortParam("timeTracking", "contentId", SortDirection.Descending),
          new SortParam("timeTracking", "createdOn", SortDirection.Ascending),
          new SortParam("timeTracking", "updatedOn", SortDirection.Ascending) };
    var order = "ORDER BY "
        + String.join(", ", Arrays.stream(sort).map(s -> s.toString("timeTracking")).toArray(String[]::new));

    var hsql = new StringBuilder();
    hsql.append(String.format("SELECT %s FROM TimeTracking timeTracking\n", select));
    hsql.append(String.format("JOIN%s timeTracking.user AS user\n", doFetch ? " FETCH" : ""));

    hsql.append(where.toString());
    if (doFetch)
      hsql.append(order);

    return hsql.toString();
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
