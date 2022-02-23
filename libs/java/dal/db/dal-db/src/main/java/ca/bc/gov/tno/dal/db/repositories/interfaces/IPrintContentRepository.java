package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.PrintContent;

/**
 * IPrintContentRepository interface, provides a way to interact with the
 * PrintContent repository.
 */
@Repository
public interface IPrintContentRepository extends JpaRepository<PrintContent, Integer> {

}