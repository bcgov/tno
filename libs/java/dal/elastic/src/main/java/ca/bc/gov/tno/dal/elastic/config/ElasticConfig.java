package ca.bc.gov.tno.dal.elastic.config;

import org.elasticsearch.client.RestHighLevelClient;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.ComponentScan;
import org.springframework.context.annotation.Configuration;
import org.springframework.data.elasticsearch.client.ClientConfiguration;
import org.springframework.data.elasticsearch.client.RestClients;
import org.springframework.data.elasticsearch.config.AbstractElasticsearchConfiguration;
import org.springframework.data.elasticsearch.core.ElasticsearchOperations;
import org.springframework.data.elasticsearch.core.ElasticsearchRestTemplate;
import org.springframework.data.elasticsearch.core.convert.ElasticsearchConverter;
import org.springframework.data.elasticsearch.repository.config.EnableElasticsearchRepositories;

/**
 * ElasticConfig class, provides configuration settings for Elasticsearch.
 */
@Configuration
@EnableElasticsearchRepositories(basePackages = "ca.bc.gov.tno.dal.elastic.repositories")
@ComponentScan(basePackages = { "ca.bc.gov.tno.dal.elastic" })
public class ElasticConfig extends AbstractElasticsearchConfiguration {
  /**
   * The URL to the Elasticsearch instance.
   */
  @Value("${elastic.url}")
  private String url;

  /**
   * The Elasticsearch username.
   */
  @Value("${elastic.username}")
  private String username;

  /**
   * The Elasticsearch password.
   */
  @Value("${elastic.password}")
  private String password;

  /**
   * Provide the HTTP client.
   * 
   * @return New instance of a RestHighLevelClient object.
   */
  @Bean
  @Override
  public RestHighLevelClient elasticsearchClient() {
    ClientConfiguration clientConfiguration = ClientConfiguration.builder().connectedTo(url)
        .withBasicAuth(username, password).build();

    return RestClients.create(clientConfiguration).rest();
  }

  /**
   * Provide a new instance of an ElasticsearchOperations.
   * 
   * @return Provide a new instance of an ElasticsearchOperations.
   */
  @Bean
  @Override
  public ElasticsearchOperations elasticsearchOperations(ElasticsearchConverter elasticsearchConverter) {
    return new ElasticsearchRestTemplate(elasticsearchClient(), elasticsearchConverter);
  }
}
