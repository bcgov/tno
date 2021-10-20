package ca.bc.gov.tno.dal.db.entities;

import java.util.Date;
import java.util.EnumSet;

import javax.persistence.Column;
import javax.persistence.Convert;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.Table;

import ca.bc.gov.tno.dal.db.Months;
import ca.bc.gov.tno.dal.db.WeekDays;
import ca.bc.gov.tno.dal.db.converters.WeekDaysAttributeConverter;
import ca.bc.gov.tno.dal.db.converters.MonthsAttributeConverter;

@Entity
@Table(name = "\"Schedules\"")
public class Schedule extends Audit {
  @Id
  @GeneratedValue(strategy = GenerationType.AUTO)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  @Column(name = "\"name\"", nullable = false)
  private String name;

  @Column(name = "\"description\"")
  private String description;

  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean isEnabled;

  @Column(name = "\"delayMS\"", nullable = false)
  private int delayMS;

  @Column(name = "\"runAt\"")
  private Date runAt;

  @Column(name = "\"runOnWeekDays\"", nullable = false)
  @Convert(converter = WeekDaysAttributeConverter.class)
  private EnumSet<WeekDays> runOnWeekDays;

  @Column(name = "\"runOnMonths\"", nullable = false)
  @Convert(converter = MonthsAttributeConverter.class)
  private EnumSet<Months> runOnMonths;

  @Column(name = "\"dayOfMonth\"", nullable = false)
  private int dayOfMonth;

  public Schedule() {

  }

  public Schedule(int id, String name) {
    this.id = id;
    this.name = name;
  }

  /**
   * @return int return the id
   */
  public int getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(int id) {
    this.id = id;
  }

  /**
   * @return String return the name
   */
  public String getName() {
    return name;
  }

  /**
   * @param name the name to set
   */
  public void setName(String name) {
    this.name = name;
  }

  /**
   * @return String return the description
   */
  public String getDescription() {
    return description;
  }

  /**
   * @param description the description to set
   */
  public void setDescription(String description) {
    this.description = description;
  }

  /**
   * @return boolean return the isEnabled
   */
  public boolean isIsEnabled() {
    return isEnabled;
  }

  /**
   * @param isEnabled the isEnabled to set
   */
  public void setIsEnabled(boolean isEnabled) {
    this.isEnabled = isEnabled;
  }

}
