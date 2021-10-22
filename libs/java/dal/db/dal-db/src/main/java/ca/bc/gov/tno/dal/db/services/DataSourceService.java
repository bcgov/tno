package ca.bc.gov.tno.dal.db.services;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.dal.db.entities.DataSource;
import ca.bc.gov.tno.dal.db.repositories.IDataSourceRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IDataSourceService;

@Service
public class DataSourceService implements IDataSourceService {

  @Autowired
  private IDataSourceRepository repository;

  @Override
  public List<DataSource> findAll() {
    var DataSources = (List<DataSource>) repository.findAll();
    return DataSources;
  }

}
