package ca.bc.gov.tno.api.editor.api;

import com.microsoft.cognitiveservices.speech.SpeechConfig;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class CognitiveServicesConfig {

  @Value("${microsoft.cognitiveservices.speech.subscription-key}")
  private String subscriptionKey;

  @Value("${microsoft.cognitiveservices.speech.region}")
  private String region;

  @Bean
  public SpeechConfig getConfig() {
    return SpeechConfig.fromSubscription(subscriptionKey, region);
  }
}
