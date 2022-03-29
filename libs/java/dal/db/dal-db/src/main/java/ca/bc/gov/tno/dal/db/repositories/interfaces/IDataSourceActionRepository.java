package ca.bc.gov.tno.dal.db.repositories.interfaces;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.DataSourceAction;
import ca.bc.gov.tno.dal.db.entities.DataSourceActionPK;

/**
 * IDataSourceActionRepository interface, provides a way to interact with the
 * DataSourceAction repository.
 */
@Repository
public interface IDataSourceActionRepository extends JpaRepository<DataSourceAction, DataSourceActionPK> {

}