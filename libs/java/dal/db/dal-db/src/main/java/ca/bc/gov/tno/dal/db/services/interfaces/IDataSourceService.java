package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;

import ca.bc.gov.tno.dal.db.entities.DataSource;

public interface IDataSourceService {
  List<DataSource> findAll();
}
