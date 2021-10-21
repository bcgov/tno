package ca.bc.gov.tno.dal.db.services;

import java.util.List;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;

import org.hibernate.Session;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.dal.db.entities.License;
import ca.bc.gov.tno.dal.db.repositories.ILicenseRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.ILicenseService;

@Service
public class LicenseService implements ILicenseService {

  @Autowired
  private ILicenseRepository repository;

  @PersistenceContext
  private EntityManager entityManager;

  @Override
  public List<License> findAll() {
    var session = (Session) entityManager.unwrap(Session.class);
    var licenses = (List<License>) repository.findAll();
    var license = session.get(License.class, licenses.get(0).getId());
    // TODO: Figure out how to do lazy loading of children.
    license.getDataSources();
    session.close();
    return licenses;
  }

}
