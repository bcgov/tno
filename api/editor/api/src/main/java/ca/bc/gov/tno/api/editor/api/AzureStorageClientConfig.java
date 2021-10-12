package ca.bc.gov.tno.api.editor.api;

import com.azure.storage.blob.BlobContainerClient;
import com.azure.storage.blob.BlobServiceClient;
import com.azure.storage.blob.BlobServiceClientBuilder;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for Azure Storage services.
 */
@Configuration
public class AzureStorageClientConfig {

  @Value("${azure.storage.connection-string}")
  private String connectionString;

  @Value("${azure.storage.container-name}")
  private String containerName;

  @Bean
  public BlobServiceClient getServiceClient() {
    return new BlobServiceClientBuilder().connectionString(connectionString).buildClient();
  }

  @Bean
  public BlobContainerClient getContainerClient() {
    BlobServiceClient client = getServiceClient();
    BlobContainerClient containerClient = client.getBlobContainerClient(containerName);

    return containerClient;
  }
}
