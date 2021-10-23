package ca.bc.gov.tno.dal.db.services.interfaces;

import java.util.List;
import java.util.Optional;

import ca.bc.gov.tno.dal.db.entities.ContentReference;
import ca.bc.gov.tno.dal.db.entities.ContentReferencePK;

public interface IContentReferenceService {
  List<ContentReference> findAll();

  Optional<ContentReference> findById(ContentReferencePK key);

  ContentReference add(ContentReference reference);

  ContentReference update(ContentReference reference);

  void delete(ContentReference reference);
}
