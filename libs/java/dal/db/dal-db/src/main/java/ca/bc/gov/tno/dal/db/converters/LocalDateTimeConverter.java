package ca.bc.gov.tno.dal.db.converters;

import java.time.LocalDateTime;

import javax.persistence.AttributeConverter;

/**
 * LocalDateTimeConverter class, provides a way to convert LocalDateTime to SQL
 * Timestamp.
 */
public class LocalDateTimeConverter implements AttributeConverter<LocalDateTime, java.sql.Timestamp> {
  /**
   * Convert LocalDateTime to SQL Timestamp.
   */
  @Override
  public java.sql.Timestamp convertToDatabaseColumn(LocalDateTime entityValue) {
    return entityValue == null ? null : java.sql.Timestamp.valueOf(entityValue);
  }

  /**
   * Convert SQL Timestamp to LocalDateTime.
   */
  @Override
  public LocalDateTime convertToEntityAttribute(java.sql.Timestamp dbValue) {
    return dbValue == null ? null : dbValue.toLocalDateTime();
  }
}
