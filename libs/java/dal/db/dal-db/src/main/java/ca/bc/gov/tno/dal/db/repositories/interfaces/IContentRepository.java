package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.Content;

/**
 * IContentRepository interface, provides a way to interact with the Content
 * repository.
 */
@Repository
public interface IContentRepository extends JpaRepository<Content, Integer> {

}