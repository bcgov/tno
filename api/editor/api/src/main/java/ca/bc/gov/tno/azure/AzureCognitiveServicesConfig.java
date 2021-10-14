package ca.bc.gov.tno.azure;

import com.microsoft.cognitiveservices.speech.SpeechConfig;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Configuration settings for Azure Cognitive services.
 */
@Configuration
public class AzureCognitiveServicesConfig {

  @Value("${azure.cognitiveservices.speech.subscription-key}")
  private String subscriptionKey;

  @Value("${azure.cognitiveservices.speech.region}")
  private String region;

  @Bean
  public SpeechConfig getConfig() {
    return SpeechConfig.fromSubscription(subscriptionKey, region);
  }
}
