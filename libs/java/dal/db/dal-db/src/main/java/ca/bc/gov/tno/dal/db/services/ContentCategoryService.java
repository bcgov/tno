package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentCategory;
import ca.bc.gov.tno.dal.db.entities.ContentCategoryPK;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IContentCategoryRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentCategoryService;

/**
 * ContentCategoryService class, provides a concrete way to interact with
 * content
 * types in the database.
 */
@Service
public class ContentCategoryService implements IContentCategoryService {

  private final SessionFactory sessionFactory;
  private final IContentCategoryRepository repository;

  /**
   * Creates a new instance of a ContentCategoryService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The content category repository.
   */
  @Autowired
  public ContentCategoryService(final SessionFactory sessionFactory, final IContentCategoryRepository repository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content category.
   */
  @Override
  public List<ContentCategory> findAll() {
    var result = (List<ContentCategory>) repository.findAll();
    return result;
  }

  /**
   * Find the content category for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content category if it exists.
   */
  @Override
  public Optional<ContentCategory> findById(ContentCategoryPK key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find all content category for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the content category if it exists.
   */
  @Override
  public List<ContentCategory> findById(int contentId) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var results = session
          .createQuery("SELECT cc FROM ContentCategory cc JOIN FETCH cc.category AS c WHERE cc.contentId=:id")
          .setParameter("id", contentId)
          .getResultList();
      return ListHelper.castList(ContentCategory.class, results);
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Add a new content category to the data source.
   * 
   * @param entity The content category to add.
   * @return A new instance of the content category that was added.
   */
  @Override
  public ContentCategory add(ContentCategory entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Add a new content category to the data source.
   * 
   * @param entities An array of content category to add.
   * @return A new instance of the content category that was added.
   */
  @Override
  public Iterable<ContentCategory> add(Iterable<ContentCategory> entities) {
    var result = repository.saveAll(
        StreamSupport.stream(entities.spliterator(), false).map((entity) -> PrincipalHelper.addAudit(entity)).toList());
    return result;
  }

  /**
   * Update the specified content category in the data source.
   * 
   * @param entity The content category to update.
   * @return A new instance of the content category that was updated.
   */
  @Override
  public ContentCategory update(ContentCategory entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Update the specified content category in the data source.
   * 
   * @param entities An array of content category to update.
   * @return A new instance of the content category that was updated.
   */
  @Override
  public Iterable<ContentCategory> update(Iterable<ContentCategory> entities) {
    var result = repository.saveAll(StreamSupport.stream(entities.spliterator(), false)
        .map((entity) -> PrincipalHelper.updateAudit(entity)).toList());
    return result;
  }

  /**
   * Delete the specified content category from the data source.
   * 
   * @param entity The content category to delete.
   */
  @Override
  public void delete(ContentCategory entity) {
    repository.delete(entity);
  }

  /**
   * Delete the specified content category from the data source.
   * 
   * @param entities An array of content category to delete.
   */
  @Override
  public void delete(Iterable<ContentCategory> entities) {
    repository.deleteAll(entities);
  }

}
