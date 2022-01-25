package ca.bc.gov.tno.dal.db.services;

import java.util.List;
import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.entities.FileReference;
import ca.bc.gov.tno.dal.db.repositories.IFileReferenceRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IFileReferenceService;

/**
 * FileReferenceService class, provides a concrete way to interact with content
 * types in the database.
 */
@Service
public class FileReferenceService implements IFileReferenceService {

  private final IFileReferenceRepository repository;

  /**
   * Creates a new instance of a FileReferenceService object, initializes with
   * specified parameters.
   * 
   * @param repository The file reference repository.
   */
  @Autowired
  public FileReferenceService(final IFileReferenceRepository repository) {
    this.repository = repository;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of file reference.
   */
  @Override
  public List<FileReference> findAll() {
    var FileReferences = (List<FileReference>) repository.findAll();
    return FileReferences;
  }

  /**
   * Find the file reference for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the file reference if it exists.
   */
  @Override
  public Optional<FileReference> findById(int key) {
    var reference = repository.findById(key);
    return reference;
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
   * Delete the specified file reference from the data source.
   * 
   * @param entity The file reference to delete.
   */
  @Override
  public void delete(FileReference entity) {
    repository.delete(entity);
  }

}
