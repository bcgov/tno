package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentTag;
import ca.bc.gov.tno.dal.db.entities.ContentTagPK;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IContentTagRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentTagService;

/**
 * ContentTagService class, provides a concrete way to interact with
 * content
 * types in the database.
 */
@Service
public class ContentTagService implements IContentTagService {

  private final SessionFactory sessionFactory;
  private final IContentTagRepository repository;

  /**
   * Creates a new instance of a ContentTagService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The content tag repository.
   */
  @Autowired
  public ContentTagService(final SessionFactory sessionFactory, final IContentTagRepository repository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content tag.
   */
  @Override
  public List<ContentTag> findAll() {
    var result = (List<ContentTag>) repository.findAll();
    return result;
  }

  /**
   * Find the content tag for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content tag if it exists.
   */
  @Override
  public Optional<ContentTag> findById(ContentTagPK key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find all content tag for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the content tag if it exists.
   */
  @Override
  public List<ContentTag> findByContentId(int contentId) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var results = session
          .createQuery("SELECT ct FROM ContentTag ct JOIN FETCH ct.tag AS t WHERE ct.contentId=:id")
          .setParameter("id", contentId)
          .getResultList();
      return ListHelper.castList(ContentTag.class, results);
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Add a new content tag to the data source.
   * 
   * @param entity The content tag to add.
   * @return A new instance of the content tag that was added.
   */
  @Override
  public ContentTag add(ContentTag entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Add a new content tag to the data source.
   * 
   * @param entities An array of content tag to add.
   * @return A new instance of the content tag that was added.
   */
  @Override
  public Iterable<ContentTag> add(Iterable<ContentTag> entities) {
    var result = repository.saveAll(
        StreamSupport.stream(entities.spliterator(), false).map((entity) -> PrincipalHelper.addAudit(entity)).toList());
    return result;
  }

  /**
   * Update the specified content tag in the data source.
   * 
   * @param entity The content tag to update.
   * @return A new instance of the content tag that was updated.
   */
  @Override
  public ContentTag update(ContentTag entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Update the specified content tag in the data source.
   * 
   * @param entities An array of content tag to update.
   * @return A new instance of the content tag that was updated.
   */
  @Override
  public Iterable<ContentTag> update(Iterable<ContentTag> entities) {
    var result = repository.saveAll(StreamSupport.stream(entities.spliterator(), false)
        .map((entity) -> PrincipalHelper.updateAudit(entity)).toList());
    return result;
  }

  /**
   * Delete the specified content tag from the data source.
   * 
   * @param entity The content tag to delete.
   */
  @Override
  public void delete(ContentTag entity) {
    repository.delete(entity);
  }

  /**
   * Delete the specified content tag from the data source.
   * 
   * @param entities An array of content tag to delete.
   */
  @Override
  public void delete(Iterable<ContentTag> entities) {
    repository.deleteAll(entities);
  }

}
