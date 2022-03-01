package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentType;

/**
 * IContentTypeRepository interface, provides a way to interact with the
 * ContentType repository.
 */
@Repository
public interface IContentTypeRepository extends JpaRepository<ContentType, Integer> {

}