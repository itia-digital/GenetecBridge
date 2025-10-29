# Anthology/UP to Genetec Mapping and Data Binding

This document describes how records fetched from Anthology/UP are projected into internal UpRecordValue and then mapped and bound to Genetec Directory tables. It lists all fields involved, default values, and transformation rules.

Scope:
- Source: Anthology (VUsuariosUnificado) and UP (PsUpIdGralTVw)
- Intermediate: Core.Data.UpRecordValue
- Target: Genetec Directory tables via EF models (Entity, Cardholder, CustomFieldValue, CardholderMembership, PartitionMembership)
- Sync pipeline: Genetec.Data.SyncWorker


Source projection (Anthology/UP ➜ UpRecordValue)
- File(s):
  - AnthologySap/Repositories/Repository.cs
  - UP.Data/Repositories/Repository.cs
- For each upstream row, we project:
  - Id: string (required)
    - Anthology: Emplid trimmed; if more than 7 chars, last 7 characters are used in Anthology repository. UP repository uses Emplid as-is and trimmed at selection time.
  - Email: string? (required in record type but value can be null)
    - Maps from Emailid
  - Name: string? (required) ➜ FirstName.Trim()
  - LastName: string? (required) ➜ string interpolation of LastName.Trim() + " " + SecondLastName.Trim()
  - Campus: string? (required) ➜ Institution.Trim()
  - GenetecGroup: Guid (required) ➜ Provided by the service caller to indicate which Genetec group to bind the cardholder to.
  - PositionOrProgram: string? (required) ➜ Descr.Trim()
  - Type: string? (required) ➜ AsgmtType (assignment type from source)
  - IsActive: bool (required) ➜ Determined by extension method .IsActive() on the source row (see AnthologySap/Extension and UP.Data/Extension).

- UpRecordValue convenience properties:
  - FullName (computed):
    - If both Name and LastName present: "{Name} {LastName}"
    - Else fallback to Name or LastName or Id


Target models and fields
- Entity (Genetec.Data/Models/Entity.cs)
  - Guid: Guid (PK)
  - Name: nvarchar(100)
  - Description: nvarchar(100)
  - Type: byte
  - SubType: byte
  - CustomType: Guid?
  - Version: int?
  - CreationTime: datetime
  - Flags: long
  - LogicalId: int?
  - Info: string?
  - CustomIcon: string?
  - HiddenFromUi: bool?
  - Federated: bool?
  - UpId: nvarchar(10) — the upstream ID for linkage
  - SyncedAt: datetime? — last sync timestamp

- Cardholder (Genetec.Data/Models/Cardholder.cs)
  - Guid: Guid (PK, also FK to Entity.Guid via navigation Gu)
  - FirstName: nvarchar(100)?
  - LastName: nvarchar(100)?
  - Picture: Guid?
  - Thumbnail: Guid?
  - Status: byte
  - ExpirationMode: byte
  - ExpirationDuration: int?
  - ExpirationDate: datetime?
  - ActivationDate: datetime?
  - Email: string?
  - AntipassbackExemption: bool
  - ExtendedGrantTime: bool
  - Info: string?
  - Escort: Guid?
  - Escort2: Guid?
  - MandatoryEscort: bool
  - CanEscort: bool
  - VisitDate: datetime?
  - MobilePhoneNumber: nvarchar(100)?
  - UpId: nvarchar(10)

- CustomFieldValue (Genetec.Data/Models/CustomFieldValue.cs)
  - Guid: Guid (PK, matches Entity.Guid)
  - UIUpId: nvarchar(500) — Column CF30fd60cbf46340be8a4e8076dcdae701
  - Campus: nvarchar(500) — Column CFabe5f7d18ca0444db8477291c3ab7bdd
  - PuestoCarreraOEspecialidad: nvarchar(500) — Column CF52978bc6661f44dc843fe4b4bdef1ba6
  - UpId: nvarchar(10) — technical linkage copy of upstream Id

- CardholderMembership (Genetec.Data/Models/CardholderMembership.cs)
  - GuidGroup: Guid (PK part) — the Genetec group entity ID
  - GuidMember: Guid (PK part) — the cardholder Entity.Guid

- PartitionMembership (Genetec.Data/Models/PartitionMembership.cs)
  - GuidGroup: Guid (PK part) — partition ID constant
  - GuidMember: Guid (PK part) — the cardholder Entity.Guid


Mapping rules (UpRecordValue ➜ Genetec)
1) Entity mapping (Genetec.Data/Mappers/EntityMapper.cs)
- Name = source.FullName
- Guid = Guid.NewGuid() (on insert)
- Type = Constants.GenetecCardHolderEntityType
- Version = Constants.GenetecDefaultEntityVersion
- CreationTime = DateTime.UtcNow
- Description = "" (empty)
- SubType = 0
- Flags = 0
- UpId = source.Id
- SyncedAt is set by SyncWorker to the current sync start time.

Upsert behavior (SyncWorker):
- Conflict key: UpId
- On update, ensures Type and Version are consistent.

2) Cardholder mapping (Genetec.Data/Mappers/CardHolderMapper.cs)
- Guid = entityId (the Entity.Guid already persisted for this UpId)
- FirstName = source.Name
- LastName = source.LastName
- Status = 0 if source.IsActive else 1 (0=Active, 1=Inactive)
- ExpirationMode = 0
- ExpirationDuration = 0
- ExpirationDate = null
- ActivationDate = DateTime.UtcNow
- Email = source.Email
- AntipassbackExemption = false
- ExtendedGrantTime = false
- Info = null
- Escort = null
- Escort2 = null
- MandatoryEscort = false
- CanEscort = false
- VisitDate = null
- MobilePhoneNumber = source.Phone ?? ""
- UpId = source.Id

Upsert behavior (SyncWorker):
- Conflict key: Guid
- On update, the following fields are refreshed: UpId, Email, Status, LastName, FirstName, MobilePhoneNumber

3) Custom field mapping (SyncWorker ➜ CustomFieldValue)
For each Id (UpRecordValue.Id), SyncWorker creates or updates one CustomFieldValue row:
- Guid = Entity.Guid (for this UpId)
- UIUpId = source.Id (shown with a leading character in README example, but code writes the raw Id)
- Campus = distinct list of Campus values for this Id, joined by ", "
- PuestoCarreraOEspecialidad = distinct list of PositionOrProgram values for this Id, joined by ", "
- UpId = source.Id

Upsert behavior:
- Conflict key: Guid
- On update, refreshes UpId, UIUpId, PuestoCarreraOEspecialidad, Campus

4) Group membership mapping (SyncWorker ➜ CardholderMembership)
- One membership per cardholder to the group designated by UpRecordValue.GenetecGroup
- GuidGroup = UpRecordValue.GenetecGroup
- GuidMember = Entity.Guid (for this UpId)

Notes:
- Code contains a commented-out block that would delete all memberships for the GuidMember prior to upsert. It is disabled to preserve custom assignments made outside the sync process.

5) Partition membership mapping (SyncWorker ➜ PartitionMembership)
- For each cardholder, the worker ensures membership to a fixed set of partitions:
  - Constants.GenetecPartitionDefault
  - Constants.GenetecPartitionMixcoac
  - Constants.GenetecPartitionCdUp
  - Constants.GenetecPartitionGdl
  - Constants.GenetecPartitionAgs
- For each partition p:
  - GuidGroup = p
  - GuidMember = Entity.Guid (for this UpId)


Null and formatting behavior
- Name/LastName/Campus/PositionOrProgram: the repositories Trim() leading/trailing spaces; LastName concatenates last + secondLast with a single space.
- FullName fallback: if either Name or LastName is missing, FullName falls back to whichever exists, otherwise Id.
- Email and Phone may be null; MobilePhoneNumber is set to empty string when Phone is null.
- UpId max length is 10 (in DB). Anthology repository truncates to 7 chars (last 7) for Emplid > 7; ensure upstream IDs fit the limit.


Constants involved
- Constants.GenetecCardHolderEntityType
- Constants.GenetecDefaultEntityVersion
- Constants.GenetecPartitionDefault
- Constants.GenetecPartitionMixcoac
- Constants.GenetecPartitionCdUp
- Constants.GenetecPartitionGdl
- Constants.GenetecPartitionAgs


End-to-end flow (summary)
1) Service obtains records from Anthology/UP repository as IAsyncEnumerable<List<UpRecordValue>> with a provided GenetecGroup Guid.
2) SyncWorker groups records by Id to aggregate multi-row attributes (Campus, PositionOrProgram) per person.
3) Entity is upserted by UpId; new Guid assigned when inserting.
4) Cardholder is upserted by Guid and linked 1:1 with Entity.Guid.
5) CustomFieldValue is upserted by Guid, storing UIUpId, Campus, and Position/Program as custom fields.
6) Memberships are ensured for the provided group and for all standard partitions.
7) Logging lists each processed UpId and full display name.


Field-by-field quick reference
- UpRecordValue.Id ➜ Entity.UpId, Cardholder.UpId, CustomFieldValue.UpId, CustomFieldValue.UIUpId
- UpRecordValue.FullName ➜ Entity.Name
- UpRecordValue.Name ➜ Cardholder.FirstName
- UpRecordValue.LastName ➜ Cardholder.LastName
- UpRecordValue.Email ➜ Cardholder.Email
- UpRecordValue.Phone ➜ Cardholder.MobilePhoneNumber (empty when null)
- UpRecordValue.Campus ➜ CustomFieldValue.Campus (aggregated, distinct, comma-separated)
- UpRecordValue.PositionOrProgram ➜ CustomFieldValue.PuestoCarreraOEspecialidad (aggregated, distinct, comma-separated)
- UpRecordValue.IsActive ➜ Cardholder.Status (0 active, 1 inactive)
- UpRecordValue.GenetecGroup ➜ CardholderMembership.GuidGroup
- Entity.Guid (derived from UpId) ➜ Cardholder.Guid, CardholderMembership.GuidMember, PartitionMembership.GuidMember, CustomFieldValue.Guid


Examples
Example values for an incoming record with Id=0001234:
- Entity
  - Name = "Juan Pérez López"
  - Guid = new Guid() on insert
  - Type = Cardholder entity type constant
  - Version = default entity version constant
  - CreationTime = now (UTC)
  - UpId = "0001234"
- Cardholder
  - Guid = Entity.Guid
  - FirstName = "Juan"
  - LastName = "Pérez López"
  - Status = 0 (if IsActive)
  - Email = "juan@example.com"
  - MobilePhoneNumber = "555-0101" or "" if null
  - UpId = "0001234"
- CustomFieldValue
  - Guid = Entity.Guid
  - UIUpId = "0001234"
  - Campus = "CDMX, MIXCOAC" (if multiple across rows)
  - PuestoCarreraOEspecialidad = "Ing. Sistemas"
  - UpId = "0001234"
- Memberships
  - CardholderMembership: GuidGroup = provided Genetec group for the source category
  - PartitionMembership: memberships to fixed partitions


Change log
- 2025-10-10: Initial version of mapping documentation added.