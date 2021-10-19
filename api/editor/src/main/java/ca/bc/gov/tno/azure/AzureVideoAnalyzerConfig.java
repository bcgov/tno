package ca.bc.gov.tno.azure;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for Azure Video Analyzer services.
 */
@Configuration
public class AzureVideoAnalyzerConfig {

  @Value("${azure.videoanalyzer.subscription-key}")
  public String subscriptionKey;

  @Value("${azure.videoanalyzer.account-id}")
  public String accountId;

  @Value("${azure.videoanalyzer.location}")
  public String location;
}
