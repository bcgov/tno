package ca.bc.gov.tno.keycloak;

import org.keycloak.adapters.springboot.KeycloakSpringBootConfigResolver;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings Keycloak.
 */
@Configuration
class KeycloakConfig {

  @Bean
  public KeycloakSpringBootConfigResolver KeycloakConfigResolver() {
    return new KeycloakSpringBootConfigResolver();
  }
}