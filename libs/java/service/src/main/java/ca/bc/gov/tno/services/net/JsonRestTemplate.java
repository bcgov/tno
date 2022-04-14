package ca.bc.gov.tno.services.net;

import java.util.ArrayList;
import java.util.List;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.datatype.jdk8.Jdk8Module;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

import org.springframework.http.client.ClientHttpRequestFactory;
import org.springframework.http.converter.HttpMessageConverter;
import org.springframework.http.converter.json.MappingJackson2HttpMessageConverter;
import org.springframework.web.client.RestTemplate;

public class JsonRestTemplate extends RestTemplate {

  public JsonRestTemplate() {
    init();
  }

  public JsonRestTemplate(ClientHttpRequestFactory clientHttpRequestFactory) {
    super(clientHttpRequestFactory);
    init();
  }

  private void init() {
    // Force a sensible JSON mapper.
    // Customize as needed for your project's definition of "sensible":
    var objectMapper = new ObjectMapper()
        .registerModule(new Jdk8Module())
        .registerModule(new JavaTimeModule())
        .configure(
            SerializationFeature.WRITE_DATES_AS_TIMESTAMPS, false);

    List<HttpMessageConverter<?>> messageConverters = new ArrayList<>();
    var jsonMessageConverter = new MappingJackson2HttpMessageConverter() {

      public boolean canRead(java.lang.Class<?> clazz,
          org.springframework.http.MediaType mediaType) {
        return true;
      }

      public boolean canRead(java.lang.reflect.Type type,
          java.lang.Class<?> contextClass,
          org.springframework.http.MediaType mediaType) {
        return true;
      }

      protected boolean canRead(
          org.springframework.http.MediaType mediaType) {
        return true;
      }
    };

    jsonMessageConverter.setObjectMapper(objectMapper);
    messageConverters.add(jsonMessageConverter);
    super.setMessageConverters(messageConverters);
  }
}
