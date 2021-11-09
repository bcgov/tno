package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.License;

/**
 * ILicenseRepository interface, provides a way to interact with the License
 * repository.
 */
@Repository
public interface ILicenseRepository extends CrudRepository<License, Integer> {

}