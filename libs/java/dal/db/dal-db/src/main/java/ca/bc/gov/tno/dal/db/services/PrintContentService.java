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
import ca.bc.gov.tno.dal.db.entities.ContentAction;
import ca.bc.gov.tno.dal.db.entities.ContentCategory;
import ca.bc.gov.tno.dal.db.entities.ContentTag;
import ca.bc.gov.tno.dal.db.entities.ContentTone;
import ca.bc.gov.tno.dal.db.entities.PrintContent;
import ca.bc.gov.tno.dal.db.models.FilterCollection;
import ca.bc.gov.tno.dal.db.models.FilterParam;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IPrintContentRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentService;
import ca.bc.gov.tno.dal.db.services.interfaces.IPrintContentService;
import ca.bc.gov.tno.models.Paged;
import ca.bc.gov.tno.models.interfaces.IPaged;

/**
 * PrintContentService class, provides a concrete way to interact with content
 * in the database.
 */
@Service
public class PrintContentService implements IPrintContentService {

  private final SessionFactory sessionFactory;
  private final IPrintContentRepository repository;
  private final IContentService contentService;

  /**
   * Creates a new instance of a ContentService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The print content repository.
   * @param contentService The content service.
   */
  @Autowired
  public PrintContentService(final SessionFactory sessionFactory, final IPrintContentRepository repository,
      final IContentService contentService) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
    this.contentService = contentService;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content.
   */
  @Override
  public List<PrintContent> findAll() {
    var result = (List<PrintContent>) repository.findAll();
    return result;
  }

  /**
   * Find a page of content that match the query.
   * 
   * @param page     The page to pull content from.
   * @param quantity Number of items to return in a page.
   * @param filter   An array of filter parameters.
   * @param sort     An array of sort parameters ['col1 desc', 'col2 asc']
   * @return A page of content.
   */
  public IPaged<PrintContent> find(int page, int quantity, FilterCollection filter, SortParam[] sort) {
    page = page < 1 ? 1 : page;
    quantity = quantity < 1 ? 10 : quantity;

    if (sort == null || sort.length == 0)
      sort = new SortParam[] {
          new SortParam("content", "createdOn", SortDirection.Descending),
          new SortParam("content", "updatedOn", SortDirection.Descending),
          new SortParam("content", "source", SortDirection.Ascending),
          new SortParam("content", "headline", SortDirection.Ascending) };

    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    // TODO: Switch to parameters.
    StringBuilder where = new StringBuilder();
    if (filter != null && filter.getFilters().size() > 0) {
      where.append(" WHERE");
      var filters = filter.getFilters();
      var first = true;
      for (Object op : filters) {
        var param = (FilterParam<?>) op;
        where.append(String.format("%s %s", (!first ? " AND" : ""), param.toString("content")));
        first = false;
      }
    }

    try {
      var order = " ORDER BY "
          + String.join(", ", Arrays.stream(sort).map(s -> s.toString("print")).toArray(String[]::new));
      var pageSql = """
          SELECT DISTINCT print FROM PrintContent print
          JOIN FETCH print.content AS content
          JOIN FETCH content.contentType AS contentType
          JOIN FETCH content.mediaType AS mediaType
          JOIN FETCH content.license AS license
          LEFT JOIN FETCH content.owner AS owner
          LEFT JOIN FETCH content.dataSource AS dataSource
          """ + where.toString() + order;
      var pageQuery = session.createQuery(pageSql)
          .setFirstResult((page - 1) * quantity)
          .setMaxResults(quantity);
      var items = pageQuery.getResultList();

      var totalSql = "SELECT COUNT(*) FROM PrintContent print" + where.toString();
      var totalQuery = session.createQuery(totalSql);
      var total = (long) totalQuery.uniqueResult();

      return new Paged<PrintContent>(ListHelper.castList(PrintContent.class, items), page, quantity, total);
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Find the content for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content if it exists.
   */
  @Override
  public Optional<PrintContent> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find the content for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content if it exists.
   */
  @Override
  public Optional<PrintContent> findById(int key, Boolean eager) {
    if (eager) {
      var session = sessionFactory.getCurrentSession();
      var ts = session.beginTransaction();
      try {

        var sql = """
            SELECT DISTINCT print FROM PrintContent print
            JOIN FETCH print.content as content
            JOIN FETCH content.contentType AS contentType
            JOIN FETCH content.mediaType AS mediaType
            JOIN FETCH content.license AS license
            LEFT JOIN FETCH content.owner AS owner
            LEFT JOIN FETCH content.series AS series
            LEFT JOIN FETCH content.dataSource AS dataSource
            LEFT JOIN FETCH content.timeTrackings AS timeTrackings
            WHERE print.content_id=:id
            """;
        var find = session.createQuery(sql)
            .setParameter("id", key);
        var result = Optional.ofNullable((PrintContent) find.uniqueResult());

        if (result.isPresent()) {
          var tags = session
              .createQuery("SELECT ct FROM ContentTag ct JOIN FETCH ct.tag AS t WHERE ct.contentId=:id")
              .setParameter("id", key)
              .getResultList();
          result.get().getContent().setContentTags(ListHelper.castList(ContentTag.class, tags));

          var actions = session
              .createQuery("SELECT ca FROM ContentAction ca JOIN FETCH ca.action AS a WHERE ca.contentId=:id")
              .setParameter("id", key)
              .getResultList();
          result.get().getContent().setContentActions(ListHelper.castList(ContentAction.class, actions));

          var categories = session
              .createQuery("SELECT cc FROM ContentCategory cc JOIN FETCH cc.category AS c WHERE cc.contentId=:id")
              .setParameter("id", key)
              .getResultList();
          result.get().getContent().setContentCategories(ListHelper.castList(ContentCategory.class, categories));

          var tonePools = session
              .createQuery("SELECT ct FROM ContentTone ct JOIN FETCH ct.tonePool AS tp WHERE ct.contentId=:id")
              .setParameter("id", key)
              .getResultList();
          result.get().getContent().setContentTonePools(ListHelper.castList(ContentTone.class, tonePools));
        }

        return result;
      } finally {
        ts.commit();
        session.close();
      }
    }

    return findById(key);
  }

  /**
   * Add a new content to the data source.
   * 
   * @param entity The content to add.
   * @return A new instance of the content that was added.
   */
  @Override
  public PrintContent add(PrintContent entity) {
    var content = contentService.add(entity.getContent());
    entity.setContentId(content.getId());
    entity.setContent(content);
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
  public PrintContent update(PrintContent entity) {
    contentService.update(entity.getContent());
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Delete the specified content from the data source.
   * 
   * @param entity The content to delete.
   */
  @Override
  public void delete(PrintContent entity) {
    contentService.delete(entity.getContent());
  }

}
