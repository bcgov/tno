package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.DataLocation;

/**
 * IDataLocationRepository interface, provides a way to interact with the
 * DataLocation repository.
 */
@Repository
public interface IDataLocationRepository extends JpaRepository<DataLocation, Integer> {

}