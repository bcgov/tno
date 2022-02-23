package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.MediaType;

/**
 * IMediaTypeRepository interface, provides a way to interact with the
 * MediaType repository.
 */
@Repository
public interface IMediaTypeRepository extends JpaRepository<MediaType, Integer> {

}