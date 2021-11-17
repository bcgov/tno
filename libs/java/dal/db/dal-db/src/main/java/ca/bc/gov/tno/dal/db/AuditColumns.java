package ca.bc.gov.tno.dal.db;

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

/**
 * AuditColumns abstract class, provides standardized audit columns for entities
 * in the DB.
 */
@MappedSuperclass
public abstract class AuditColumns implements Serializable {
    /**
     * The uid that identifies the user who created the record.
     */
    @Column(name = "\"createdById\"", nullable = false)
    private UUID createdById;

    /**
     * A name that identifies the user who created the record.
     */
    @Column(name = "\"createdBy\"", nullable = false)
    private String createdBy;

    /**
     * When the record was created. Automatically set by the DB.
     */
    @Source(SourceType.DB)
    @Generated(GenerationTime.ALWAYS)
    @Temporal(TemporalType.TIMESTAMP)
    @Column(name = "\"createdOn\"", nullable = false)
    private Date createdOn;

    /**
     * The uid that identifies the user who updated the record last.
     */
    @Column(name = "\"updatedById\"", nullable = false)
    private UUID updatedById;

    /**
     * The name that identifies the user who updated the record last.
     */
    @Column(name = "\"updatedBy\"", nullable = false)
    private String updatedBy;

    /**
     * When the record was last updated. Automatically set by the DB. Provides
     * concurrency control to enforce optimistic concurrency.
     */
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

}
