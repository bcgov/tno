package ca.bc.gov.tno;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

@EnableJpaRepositories(basePackages = { "ca.bc.gov.tno.dal.*" })
@EntityScan(basePackages = { "ca.bc.gov.tno.dal.*" })
@SpringBootApplication(scanBasePackages = { "ca.bc.gov.tno", "ca.bc.gov.tno.keycloak", "ca.bc.gov.tno.dal.*",
		"ca.bc.gov.tno.azure", "ca.bc.gov.tno.elastic", "ca.bc.gov.tno.kafka" })
public class ApiEditor {

	public static void main(String[] args) {
		SpringApplication.run(ApiEditor.class, args);
	}

}
