package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.FileReference;

/**
 * IFileReferenceRepository interface, provides a way to interact with the
 * FileReference
 * repository.
 */
@Repository
public interface IFileReferenceRepository extends JpaRepository<FileReference, Integer> {

}