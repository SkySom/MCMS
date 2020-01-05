package com.mcms.api.datamodel.core;

import java.sql.Timestamp;
import java.time.Instant;
import java.util.UUID;

import org.springframework.data.annotation.Id;
import org.springframework.data.annotation.PersistenceConstructor;
import org.springframework.data.relational.core.mapping.Table;

/**
 * Represents a single mapping type.
 * So for example from OBF to SRG or from OBF to Intermediary or from SRG to MCP.
 */
@Table("mapping_type")
public class MappingTypeDMO {

    @Id
    private final UUID id;
    private final UUID      createdBy;
    private final Timestamp createdOn;
    private final String    name;

    @PersistenceConstructor
    MappingTypeDMO(final UUID id, final UUID createdBy, final Timestamp createdOn, final String name) {
        this.id = id;
        this.createdBy = createdBy;
        this.createdOn = createdOn;
        this.name = name;
    }

    public MappingTypeDMO(final UUID createdBy, final String name) {
        this.id = UUID.randomUUID();
        this.createdBy = createdBy;
        this.createdOn = Timestamp.from(Instant.now());
        this.name = name;
    }

    public UUID getId() {
        return id;
    }

    public UUID getCreatedBy() {
        return createdBy;
    }

    public Timestamp getCreatedOn() {
        return createdOn;
    }

    public String getName() {
        return name;
    }
}