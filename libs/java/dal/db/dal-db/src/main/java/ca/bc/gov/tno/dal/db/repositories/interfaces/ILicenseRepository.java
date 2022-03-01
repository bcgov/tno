package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.License;

/**
 * ILicenseRepository interface, provides a way to interact with the License
 * repository.
 */
@Repository
public interface ILicenseRepository extends JpaRepository<License, Integer> {

}