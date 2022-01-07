package ca.bc.gov.tno.elastic;

import org.elasticsearch.client.RestHighLevelClient;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.data.elasticsearch.client.ClientConfiguration;
import org.springframework.data.elasticsearch.client.RestClients;
import org.springframework.data.elasticsearch.config.AbstractElasticsearchConfiguration;

/**
 * Configuration settings for Elasticsearch.
 */
@Configuration
public class ElasticClientConfig extends AbstractElasticsearchConfiguration {
  @Value("${spring.elasticsearch.username}")
  private String username;

  @Value("${spring.elasticsearch.password}")
  private String password;

  @Value("${spring.elasticsearch.uris}")
  private String uris;

  @Override
  @Bean
  public RestHighLevelClient elasticsearchClient() {

    final ClientConfiguration clientConfiguration = ClientConfiguration.builder()
        .connectedTo(uris)
        .withBasicAuth(username, password)
        .build();

    return RestClients.create(clientConfiguration).rest();
  }
}
