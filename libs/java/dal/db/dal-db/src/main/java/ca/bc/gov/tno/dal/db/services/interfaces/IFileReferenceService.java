package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.FileReference;

/**
 * IFileReferenceService interface, provides a way to interact with file
 * references.
 */
public interface IFileReferenceService {
  /**
   * Find all that match the criteria.
   * 
   * @return A list of file reference.
   */
  List<FileReference> findAll();

  /**
   * Find the file reference for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the file reference if it exists.
   */
  Optional<FileReference> findById(int key);

  /**
   * Find all file reference for the specified content primary key.
   * 
   * @param contentId The content primary key.
   * @return A new instance of the file reference if it exists.
   */
  List<FileReference> findByContentId(int contentId);

  /**
   * Add a new file reference to the data source.
   * 
   * @param entity The file reference to add.
   * @return A new instance of the file reference that was added.
   */
  FileReference add(FileReference entity);

  /**
   * Add a new file reference to the data source.
   * 
   * @param entities An array of file reference to add.
   * @return A new instance of the file reference that was added.
   */
  Iterable<FileReference> add(Iterable<FileReference> entities);

  /**
   * Update the specified file reference in the data source.
   * 
   * @param entity The file reference to update.
   * @return A new instance of the file reference that was updated.
   */
  FileReference update(FileReference entity);

  /**
   * Update the specified file reference in the data source.
   * 
   * @param entities An array of file reference to update.
   * @return A new instance of the file reference that was updated.
   */
  Iterable<FileReference> update(Iterable<FileReference> entities);

  /**
   * Delete the specified file reference from the data source.
   * 
   * @param entity The file reference to delete.
   */
  void delete(FileReference entity);

  /**
   * Delete the specified file reference from the data source.
   * 
   * @param entities An array of file reference to delete.
   */
  void delete(Iterable<FileReference> entities);
}
