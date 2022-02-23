package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.FileReference;
import ca.bc.gov.tno.dal.db.repositories.interfaces.IFileReferenceRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IFileReferenceService;

/**
 * FileReferenceService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class FileReferenceService implements IFileReferenceService {

  private final SessionFactory sessionFactory;
  private final IFileReferenceRepository repository;

  /**
   * Creates a new instance of a FileReferenceService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory The session factory.
   * @param repository     The file reference repository.
   */
  @Autowired
  public FileReferenceService(final SessionFactory sessionFactory, final IFileReferenceRepository repository) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of file reference.
   */
  @Override
  public List<FileReference> findAll() {
    var result = (List<FileReference>) repository.findAll();
    return result;
  }

  /**
   * Find the file reference for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the file reference if it exists.
   */
  @Override
  public Optional<FileReference> findById(int key) {
    var result = repository.findById(key);
    return result;
  }

  /**
   * Find all file reference for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the file reference if it exists.
   */
  @Override
  public List<FileReference> findByContentId(int contentId) {
    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    try {
      var results = session
          .createQuery("SELECT fr FROM FileReference fr WHERE fr.contentId=:id")
          .setParameter("id", contentId)
          .getResultList();
      return ListHelper.castList(FileReference.class, results);
    } finally {
      ts.commit();
      session.close();
    }
  }

  /**
   * Add a new file reference to the data source.
   * 
   * @param entity The file reference to add.
   * @return A new instance of the file reference that was added.
   */
  @Override
  public FileReference add(FileReference entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));
    return result;
  }

  /**
   * Add a new file reference to the data source.
   * 
   * @param entities An array of file reference to add.
   * @return A new instance of the file reference that was added.
   */
  @Override
  public Iterable<FileReference> add(Iterable<FileReference> entities) {
    var result = repository.saveAll(
        StreamSupport.stream(entities.spliterator(), false).map((entity) -> PrincipalHelper.addAudit(entity)).toList());
    return result;
  }

  /**
   * Update the specified file reference in the data source.
   * 
   * @param entity The file reference to update.
   * @return A new instance of the file reference that was updated.
   */
  @Override
  public FileReference update(FileReference entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));
    return result;
  }

  /**
   * Update the specified file reference in the data source.
   * 
   * @param entities An array of file reference to update.
   * @return A new instance of the file reference that was updated.
   */
  @Override
  public Iterable<FileReference> update(Iterable<FileReference> entities) {
    var result = repository.saveAll(StreamSupport.stream(entities.spliterator(), false)
        .map((entity) -> PrincipalHelper.updateAudit(entity)).toList());
    return result;
  }

  /**
   * Delete the specified file reference from the data source.
   * 
   * @param entity The file reference to delete.
   */
  @Override
  public void delete(FileReference entity) {
    repository.delete(entity);
  }

  /**
   * Delete the specified file reference from the data source.
   * 
   * @param entities An array of file reference to delete.
   */
  @Override
  public void delete(Iterable<FileReference> entities) {
    repository.deleteAll(entities);
  }

}
