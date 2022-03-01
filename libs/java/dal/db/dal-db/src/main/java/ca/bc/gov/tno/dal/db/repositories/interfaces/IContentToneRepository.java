package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.ContentTone;
import ca.bc.gov.tno.dal.db.entities.ContentTonePK;

/**
 * IContentToneRepository interface, provides a way to interact with the
 * ContentTone repository.
 */
@Repository
public interface IContentToneRepository extends JpaRepository<ContentTone, ContentTonePK> {

}