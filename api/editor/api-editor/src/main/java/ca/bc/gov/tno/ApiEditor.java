/**
 * API Editor provides a RESTful web service for the TNO application.
 */
package ca.bc.gov.tno;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

/**
 * ApiEditor class, provides endpoints for the editor web application,
 * and provides secure access to data sources and 3rd party services.
 */
@EnableJpaRepositories(basePackages = { "ca.bc.gov.tno.dal.db" })
@EntityScan(basePackages = { "ca.bc.gov.tno.dal.db" })
@SpringBootApplication(scanBasePackages = {
		"ca.bc.gov.tno",
		"ca.bc.gov.tno.keycloak",
		"ca.bc.gov.tno.dal.db",
		"ca.bc.gov.tno.azure",
		"ca.bc.gov.tno.elastic",
		"ca.bc.gov.tno.kafka" })
public final class ApiEditor {

	/**
	 * Start the API web-server.
	 * 
	 * @param args Command line arguments.
	 */
	public static void main(final String[] args) {
		SpringApplication.run(ApiEditor.class, args);
	}

}
