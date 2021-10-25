package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.License;

public interface ILicenseService {
  List<License> findAll();

  Optional<License> findById(Integer key);

  License add(License entity);

  License update(License entity);

  void delete(License entity);
}
