package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.DataSource;

/**
 * IDataSourceRepository interface, provides a way to interact with the
 * DataSource repository.
 */
@Repository
public interface IDataSourceRepository extends JpaRepository<DataSource, Integer> {

}