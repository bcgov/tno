package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Tag;

/**
 * ITagRepository interface, provides a way to interact with the
 * Tag repository.
 */
@Repository
public interface ITagRepository extends JpaRepository<Tag, String> {

}