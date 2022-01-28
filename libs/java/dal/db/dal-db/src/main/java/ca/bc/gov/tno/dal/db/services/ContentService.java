package ca.bc.gov.tno.dal.db.services;

import java.util.Arrays;
import java.util.List;
import java.util.Optional;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.SortDirection;
import ca.bc.gov.tno.dal.db.entities.Content;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.dal.db.repositories.IContentRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentService;
import ca.bc.gov.tno.models.Paged;
import ca.bc.gov.tno.models.interfaces.IPaged;

/**
 * ContentService class, provides a concrete way to interact with content
 * in the database.
 */
@Service
public class ContentService implements IContentService {

  private final SessionFactory sessionFactory;
  private final IContentRepository repository;

  /**
   * Creates a new instance of a ContentService object, initializes with
   * specified parameters.
   * 
   * @param repository     The content repository.
   * @param sessionFactory The session factory.
   */
  @Autowired
  public ContentService(final IContentRepository repository, final SessionFactory sessionFactory) {
    this.repository = repository;
    this.sessionFactory = sessionFactory;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content.
   */
  @Override
  public List<Content> findAll() {
    var items = (List<Content>) repository.findAll();
    return items;
  }

  /**
   * Find a page of content that match the query.
   * 
   * @param page     The page to pull content from.
   * @param quantity Number of items to return in a page.
   * @param sort     An array of sort parameters ['col1 desc', 'col2 asc']
   * @return A page of content.
   */
  public IPaged<Content> find(int page, int quantity, SortParam[] sort) {
    page = page < 1 ? 1 : page;
    quantity = quantity < 1 ? 10 : quantity;

    if (sort == null || sort.length == 0)
      sort = new SortParam[] {
          new SortParam("createdOn", SortDirection.Descending),
          new SortParam("updatedOn", SortDirection.Descending),
          new SortParam("source", SortDirection.Ascending),
          new SortParam("headline", SortDirection.Ascending) };

    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var order = String.join(", ", Arrays.stream(sort).map(s -> "c." + s).toArray(String[]::new));
      var pageSql = """
          SELECT DISTINCT c FROM Content c
          JOIN FETCH c.contentType AS ct
          JOIN FETCH c.mediaType AS mt
          JOIN FETCH c.owner AS o
          JOIN FETCH c.license AS l
          LEFT JOIN FETCH c.dataSource AS ds
          ORDER BY
           """ + order;
      var pageQuery = session.createQuery(pageSql)
          .setFirstResult((page - 1) * quantity)
          .setMaxResults(quantity);
      var items = pageQuery.getResultList();

      var totalSql = "SELECT COUNT(*) FROM Content";
      var totalQuery = session.createQuery(totalSql);
      var total = (long) totalQuery.uniqueResult();

      return new Paged<Content>(ListHelper.castList(Content.class, items), page, quantity, total);
    } finally {
      ts.commit();
    }
  }

  /**
   * Find the content for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content if it exists.
   */
  @Override
  public Optional<Content> findById(int key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Add a new content to the data source.
   * 
   * @param entity The content to add.
   * @return A new instance of the content that was added.
   */
  @Override
  public Content add(Content entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Update the specified content in the data source.
   * 
   * @param entity The content to update.
   * @return A new instance of the content that was updated.
   */
  @Override
  public Content update(Content entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content from the data source.
   * 
   * @param entity The content to delete.
   */
  @Override
  public void delete(Content entity) {
    repository.delete(entity);
  }

}
