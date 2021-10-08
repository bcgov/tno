package ca.bc.gov.tno.api.editor.api;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;

@Configuration
public class AzureVideoAnalyzerConfig {

  @Value("${azure.videoanalyzer.subscription-key}")
  public String subscriptionKey;

  @Value("${azure.videoanalyzer.account-id}")
  public String accountId;

  @Value("${azure.videoanalyzer.location}")
  public String location;
}
