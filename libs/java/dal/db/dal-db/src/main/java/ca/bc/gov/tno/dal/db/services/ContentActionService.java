package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.ContentAction;
import ca.bc.gov.tno.dal.db.entities.ContentActionPK;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IContentActionRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentActionService;

/**
 * ContentActionService class, provides a concrete way to interact with
 * content
 * types in the database.
 */
@Service
public class ContentActionService implements IContentActionService {

  private final SessionFactory sessionFactory;
  private final IContentActionRepository repository;

  /**
   * Creates a new instance of a ContentActionService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The content action repository.
   */
  @Autowired
  public ContentActionService(final SessionFactory sessionFactory, final IContentActionRepository repository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content action.
   */
  @Override
  public List<ContentAction> findAll() {
    var result = (List<ContentAction>) repository.findAll();
    return result;
  }

  /**
   * Find the content action for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content action if it exists.
   */
  @Override
  public Optional<ContentAction> findById(ContentActionPK key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find all content action for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the content action if it exists.
   */
  @Override
  public List<ContentAction> findById(int contentId) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var results = session
          .createQuery("SELECT ca FROM ContentAction ca JOIN FETCH ca.action AS a WHERE ca.contentId=:id")
          .setParameter("id", contentId)
          .getResultList();
      return ListHelper.castList(ContentAction.class, results);
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Add a new content action to the data source.
   * 
   * @param entity The content action to add.
   * @return A new instance of the content action that was added.
   */
  @Override
  public ContentAction add(ContentAction entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Add a new content action to the data source.
   * 
   * @param entities An array of content action to add.
   * @return A new instance of the content action that was added.
   */
  @Override
  public Iterable<ContentAction> add(Iterable<ContentAction> entities) {
    var result = repository.saveAll(
        StreamSupport.stream(entities.spliterator(), false).map((entity) -> PrincipalHelper.addAudit(entity)).toList());
    return result;
  }

  /**
   * Update the specified content action in the data source.
   * 
   * @param entity The content action to update.
   * @return A new instance of the content action that was updated.
   */
  @Override
  public ContentAction update(ContentAction entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Update the specified content action in the data source.
   * 
   * @param entities An array of content action to update.
   * @return A new instance of the content action that was updated.
   */
  @Override
  public Iterable<ContentAction> update(Iterable<ContentAction> entities) {
    var result = repository.saveAll(StreamSupport.stream(entities.spliterator(), false)
        .map((entity) -> PrincipalHelper.updateAudit(entity)).toList());
    return result;
  }

  /**
   * Delete the specified content action from the data source.
   * 
   * @param entity The content action to delete.
   */
  @Override
  public void delete(ContentAction entity) {
    repository.delete(entity);
  }

  /**
   * Delete the specified content action from the data source.
   * 
   * @param entities An array of content action to delete.
   */
  @Override
  public void delete(Iterable<ContentAction> entities) {
    repository.deleteAll(entities);
  }

}
