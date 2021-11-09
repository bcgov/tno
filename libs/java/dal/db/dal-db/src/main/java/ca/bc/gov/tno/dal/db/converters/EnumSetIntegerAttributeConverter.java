package ca.bc.gov.tno.dal.db.converters;

import java.util.EnumSet;
import java.util.stream.IntStream;

import javax.persistence.AttributeConverter;

import ca.bc.gov.tno.IEnumValue;

/**
 * Convert an EnumSet to and from an Integer. This supports Flags, which enables
 * bitwise operator comparisons.
 */
public class EnumSetIntegerAttributeConverter<T extends Enum<T>> implements AttributeConverter<EnumSet<T>, Integer> {
  private Class<T> typeParameterClass;

  /**
   * Convert to the database column value.
   * 
   * @param attribute The entity property value.
   */
  @Override
  @SuppressWarnings("unchecked")
  public Integer convertToDatabaseColumn(EnumSet<T> attribute) {
    if (attribute == null)
      return null;

    var values = new int[attribute.size()];
    var index = 0;
    for (var a : attribute) {
      values[index++] = ((IEnumValue<Integer>) a).getValue();
    }
    var sum = IntStream.of(values).sum();
    return sum;
  }

  /**
   * Convert to the entity property value.
   * 
   * @param dbData The database column value.
   */
  @Override
  @SuppressWarnings("unchecked")
  public EnumSet<T> convertToEntityAttribute(Integer dbData) {
    if (dbData == null)
      return null;

    var all = getValues(typeParameterClass);
    var values = EnumSet.noneOf(typeParameterClass);
    for (var a : all) {
      var value = ((IEnumValue<Integer>) a).getValue();
      if (value == (dbData & value)) {
        values.add(a);
      }
    }

    return values;
  }

  /**
   * Get all the enum values from the specified type.
   * 
   * @param <T>      Type of enum
   * @param enumType The enum
   * @return An array of the enum values
   */
  public static <T extends Enum<T>> T[] getValues(Class<T> enumType) {
    return (T[]) enumType.getEnumConstants();
  }
}
