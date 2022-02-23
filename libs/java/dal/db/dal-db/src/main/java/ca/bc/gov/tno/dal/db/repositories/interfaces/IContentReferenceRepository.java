package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;

/**
 * IContentReferenceRepository interface, provides a way to interact with the
 * ContentReference repository.
 */
@Repository
public interface IContentReferenceRepository extends JpaRepository<ContentReference, ContentReferencePK> {

}