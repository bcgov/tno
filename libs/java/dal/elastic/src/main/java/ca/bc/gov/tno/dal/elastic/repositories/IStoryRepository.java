package ca.bc.gov.tno.dal.elastic.repositories;

import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.elasticsearch.annotations.Query;
import org.springframework.data.elasticsearch.repository.ElasticsearchRepository;

import ca.bc.gov.tno.dal.elastic.models.Story;

/**
 * IStoryRepository interface, provides a way to manage and query the stories
 * within Elasticsearch.
 */
public interface IStoryRepository extends ElasticsearchRepository<Story, String> {
  /**
   * Find all stories with the specified 'author'.
   * 
   * @param author   The author's name.
   * @param pageable The paging information.
   * @return A page of stories that match the filter.
   */
  Page<Story> findByAuthor(final String author, final Pageable pageable);

  /**
   * Find all stories with the specified 'author'.
   * 
   * @param author   The author's name.
   * @param pageable The paging information.
   * @return A page of stories that match the filter.
   */
  @Query("{\"bool\": {\"must\": [{\"match\": {\"author\": \"?0\"}}]}}")
  Page<Story> findByAuthorUsingCustomQuery(final String author, final Pageable pageable);
}