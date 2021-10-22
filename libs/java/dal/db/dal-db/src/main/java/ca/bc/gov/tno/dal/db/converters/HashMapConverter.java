package ca.bc.gov.tno.dal.db.converters;

import java.util.HashMap;
import java.util.Map;

import javax.persistence.AttributeConverter;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

/**
 * Convert HashMap to a String and vise-versa. Used for JSON data types.
 */
public class HashMapConverter implements AttributeConverter<Map<String, Object>, String> {

  @Override
  public String convertToDatabaseColumn(Map<String, Object> attribute) {
    if (attribute == null) {
      return null;
    }

    try {
      var objectMapper = new ObjectMapper();
      return objectMapper.writeValueAsString(attribute);
    } catch (JsonProcessingException e) {
      // TODO: Handle exception.
      e.printStackTrace();
      return null;
    }
  }

  @Override
  @SuppressWarnings("unchecked")
  public Map<String, Object> convertToEntityAttribute(String dbData) {
    if (dbData == null) {
      return new HashMap<>();
    }

    try {
      var objectMapper = new ObjectMapper();
      return objectMapper.readValue(dbData, HashMap.class);
    } catch (JsonProcessingException e) {
      // TODO: Handle exception.
      e.printStackTrace();
      return null;
    }
  }

}
