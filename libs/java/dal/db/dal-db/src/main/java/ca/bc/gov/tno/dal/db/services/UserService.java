package ca.bc.gov.tno.dal.db.services;

import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.stream.Collectors;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.SortDirection;
import ca.bc.gov.tno.dal.db.entities.TimeTracking;
import ca.bc.gov.tno.dal.db.entities.User;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IUserRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IUserService;
import ca.bc.gov.tno.models.Paged;
import ca.bc.gov.tno.models.interfaces.IPaged;

/**
 * RoleService class, provides a concrete way to interact with users in the
 * database.
 */
@Service
public class UserService implements IUserService {

  private final SessionFactory sessionFactory;
  private IUserRepository repository;

  /**
   * Creates a new instance of a UserService object, initializes with specified
   * parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The user repository.
   */
  @Autowired
  public UserService(final SessionFactory sessionFactory, final IUserRepository repository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of users.
   */
  @Override
  public List<User> findAll() {
    var result = (List<User>) repository.findAll();
    return result;
  }

  /**
   * Find a page of users that match the query.
   * 
   * @param page     The page to pull users from.
   * @param quantity Number of items to return in a page.
   * @param sort     An array of sort parameters ['col1 desc', 'col2 asc']
   * @return A page of users.
   */
  public IPaged<User> find(int page, int quantity, SortParam[] sort) {
    page = page < 1 ? 1 : page;
    quantity = quantity < 1 ? 10 : quantity;

    if (sort == null || sort.length == 0)
      sort = new SortParam[] {
          new SortParam("username", SortDirection.Ascending),
          new SortParam("lastName", SortDirection.Ascending),
          new SortParam("updatedOn", SortDirection.Descending),
          new SortParam("createdOn", SortDirection.Descending) };

    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var order = String.join(", ", Arrays.stream(sort).map(s -> "c." + s).toArray(String[]::new));
      var pageSql = """
          SELECT DISTINCT u FROM User u
          ORDER BY
           """ + order;
      var pageQuery = session.createQuery(pageSql)
          .setFirstResult((page - 1) * quantity)
          .setMaxResults(quantity);
      var items = pageQuery.getResultList();

      var totalSql = "SELECT COUNT(*) FROM User";
      var totalQuery = session.createQuery(totalSql);
      var total = (long) totalQuery.uniqueResult();

      return new Paged<User>(ListHelper.castList(User.class, items), page, quantity, total);
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Find the user for the specified primary key.
   * 
   * @param key The primary key.
   * @return The user if it exists.
   */
  @Override
  public Optional<User> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find the user for the specified unique username.
   * 
   * @param username The unique username.
   * @return The user if it exists.
   */
  @Override
  public Optional<User> findByUsername(String username) {
    var result = repository.findByUsername(username);
    return result;
  }

  /**
   * Find all time tracking for the specified time period, grouped by user.
   * 
   * @param from Content updatedOn 'from' filter.
   * @param to   Content updatedOn 'to' filter.
   * @return A map of all users who updated content within the filter and their
   *         time tracking.
   */
  @Override
  public Map<User, List<TimeTracking>> getTimeTracking(ZonedDateTime from, ZonedDateTime to) {

    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {

      var dFormat = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss Z");
      var hsql = new StringBuilder();
      hsql.append("SELECT timeTracking FROM TimeTracking timeTracking\n");
      hsql.append("JOIN timeTracking.content AS content\n");
      hsql.append("JOIN FETCH timeTracking.user AS user\n");
      hsql.append(String.format("WHERE content.updatedOn >= '%s' AND content.updatedOn <= '%s'\n",
          dFormat.format(from),
          dFormat.format(to)));

      var timeTracking = session.createQuery(hsql.toString());
      var items = ListHelper.castList(TimeTracking.class, timeTracking.getResultList());
      var users = items.stream().collect(Collectors.groupingBy(TimeTracking::getUser));

      return users;
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Add a new user to the data source.
   * 
   * @param entity The user to add.
   * @return A new instance of the user that was added.
   */
  @Override
  public User add(User entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified user in the data source.
   * 
   * @param entity The user to update.
   * @return A new instance of the user that was updated.
   */
  @Override
  public User update(User entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified user from the data source.
   * 
   * @param entity The user to delete.
   */
  @Override
  public void delete(User entity) {
    repository.delete(entity);
  }

}
