package ca.bc.gov.tno.dal.db.converters;

import java.time.LocalDateTime;

import javax.persistence.AttributeConverter;

public class LocalDateTimeConverter implements AttributeConverter<LocalDateTime, java.sql.Timestamp> {
  @Override
  public java.sql.Timestamp convertToDatabaseColumn(LocalDateTime entityValue) {
    return entityValue == null ? null : java.sql.Timestamp.valueOf(entityValue);
  }

  @Override
  public LocalDateTime convertToEntityAttribute(java.sql.Timestamp dbValue) {
    return dbValue == null ? null : dbValue.toLocalDateTime();
  }
}
