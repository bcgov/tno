package ca.bc.gov.tno.dal.elastic.services;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.dal.elastic.models.Story;
import ca.bc.gov.tno.dal.elastic.repositories.IStoryRepository;

/**
 * StoryService class, provides an api to manage the story index within
 * Elasticsearch.
 */
@Service
public class StoryService {
  private final IStoryRepository repository;

  /**
   * Creates a new instance of a StoryService object, initializes with specified
   * parameters.
   * 
   * @param repository
   */
  @Autowired
  public StoryService(IStoryRepository repository) {
    this.repository = repository;
  }

  /**
   * Save the specified story in the index.
   * 
   * @param story The story document to save.
   */
  public Story save(final Story story) {
    return repository.save(story);
  }

  /**
   * Find the story with the specified 'id'.
   * 
   * @param id The Elasticsearch key.
   * @return The story or null if not found.
   */
  public Story findById(final String id) {
    return repository.findById(id).orElse(null);
  }
}
