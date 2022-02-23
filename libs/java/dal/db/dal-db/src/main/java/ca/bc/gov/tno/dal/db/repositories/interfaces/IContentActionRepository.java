package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentAction;
import ca.bc.gov.tno.dal.db.entities.ContentActionPK;

/**
 * IContentActionRepository interface, provides a way to interact with the
 * ContentAction repository.
 */
@Repository
public interface IContentActionRepository extends JpaRepository<ContentAction, ContentActionPK> {

}