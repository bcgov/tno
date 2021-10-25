package ca.bc.gov.tno.dal.db.entities;

import java.io.Serializable;
import java.util.Date;
import java.util.UUID;

import javax.persistence.Column;
import javax.persistence.MappedSuperclass;
import javax.persistence.Temporal;
import javax.persistence.TemporalType;
import javax.persistence.Version;

import org.hibernate.annotations.Generated;
import org.hibernate.annotations.GenerationTime;
import org.hibernate.annotations.Source;
import org.hibernate.annotations.SourceType;

@MappedSuperclass
public abstract class AuditColumns implements Serializable {
    @Column(name = "\"createdById\"", nullable = false)
    private UUID createdById;

    @Column(name = "\"createdBy\"", nullable = false)
    private String createdBy;

    @Source(SourceType.DB)
    @Generated(GenerationTime.ALWAYS)
    @Temporal(TemporalType.TIMESTAMP)
    @Column(name = "\"createdOn\"", nullable = false)
    private Date createdOn;

    @Column(name = "\"updatedById\"", nullable = false)
    private UUID updatedById;

    @Column(name = "\"updatedBy\"", nullable = false)
    private String updatedBy;

    @Version
    @Source(SourceType.DB)
    @Generated(GenerationTime.ALWAYS)
    @Temporal(TemporalType.TIMESTAMP)
    @Column(name = "\"updatedOn\"", nullable = false)
    private Date updatedOn;

    /**
     * @return UUID return the createdById
     */
    public UUID getCreatedById() {
        return createdById;
    }

    /**
     * @param createdById the createdById to set
     */
    public void setCreatedById(UUID createdById) {
        this.createdById = createdById;
    }

    /**
     * @return String return the createdBy
     */
    public String getCreatedBy() {
        return createdBy;
    }

    /**
     * @param createdBy the createdBy to set
     */
    public void setCreatedBy(String createdBy) {
        this.createdBy = createdBy;
    }

    /**
     * @return Date return the createdOn
     */
    public Date getCreatedOn() {
        return createdOn;
    }

    /**
     * @param createdOn the createdOn to set
     */
    public void setCreatedOn(Date createdOn) {
        this.createdOn = createdOn;
    }

    /**
     * @return UUID return the updatedById
     */
    public UUID getUpdatedById() {
        return updatedById;
    }

    /**
     * @param updatedById the updatedById to set
     */
    public void setUpdatedById(UUID updatedById) {
        this.updatedById = updatedById;
    }

    /**
     * @return String return the updatedBy
     */
    public String getUpdatedBy() {
        return updatedBy;
    }

    /**
     * @param updatedBy the updatedBy to set
     */
    public void setUpdatedBy(String updatedBy) {
        this.updatedBy = updatedBy;
    }

    /**
     * @return Date return the updatedOn
     */
    public Date getUpdatedOn() {
        return updatedOn;
    }

    /**
     * @param updatedOn the updatedOn to set
     */
    public void setUpdatedOn(Date updatedOn) {
        this.updatedOn = updatedOn;
    }

}
