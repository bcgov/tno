package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.FileReference;

/**
 * IFileReferenceRepository interface, provides a way to interact with the
 * FileReference
 * repository.
 */
@Repository
public interface IFileReferenceRepository extends CrudRepository<FileReference, Integer> {

}