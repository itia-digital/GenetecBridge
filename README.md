## Update code in UP server

```shell
dotnet publish -c Release -r win-x64 --self-contained true -o "C:\Alusa" /p:PublishSingleFile=true
```

## Insert a new cardholder in genetec
Trace
```sql
-- Entity
INSERT INTO Entity (Guid,Type,SubType,Flags,CustomType,Name,Description,CreationTime,LogicalID,CustomIcon,Info,Version)
values('0282574A-F92C-4FF7-A443-D464FCFED598',7,0,0,NULL,N'028257 ROBOT MARTINEZ',N'ROBOT MI DESCRIPCION...','2025-01-16 22:05:01',NULL,NULL,NULL,23296)

-- Cardholder
INSERT INTO Cardholder (Guid,Status,ExpirationMode,ExpirationDuration,ExpirationDate,ActivationDate,AntipassbackExemption,ExtendedGrantTime,Info,Escort,Escort2,MandatoryEscort,CanEscort,VisitDate,FirstName,LastName,Picture,Email,Thumbnail,MobilePhoneNumber)
values('0282574A-F92C-4FF7-A443-D464FCFED598',0,0,0,NULL,'2025-01-16 22:05:01',0,0,NULL,NULL,NULL,0,0,NULL,N'MANOLO',N'MARTINEZ',NULL,N'manolo@itia.mx',NULL,N'5559648139')

-- CardholderMembership
INSERT INTO CardholderMembership
values('17DDEE1D-6A08-440F-A79B-8E049C93C289','0282574A-F92C-4FF7-A443-D464FCFED598')

----- CustomFieldValue
insert into CustomFieldValue (Guid, CF30fd60cbf46340be8a4e8076dcdae701, CFabe5f7d18ca0444db8477291c3ab7bdd)
values ('0282574A-F92C-4FF7-A443-D464FCFED598', '}IMSS', 'CLAVE');
```


## Database

### Genetec database
Scaffolding:
```shell
dotnet ef dbcontext scaffold "Server=172.25.15.123\ACCESOS;Database=Directory1;TrustServerCertificate=True;User ID=genetec;Password=genetec" Microsoft.EntityFrameworkCore.SqlServer -o Models -t Entity -t Cardholder -t CardholderMembership -t AlusaControl --context GenetecDbContext --data-annotations --nullable
```

```sql
create table AlusaControl
(
    Name      nvarchar(50),
    StartedAt datetime,
    EndedAt   datetime,
    Id        int identity
)
go

create index AlusaControl_Name_EndedAt_index
    on AlusaControl (Name, EndedAt)
go


alter table dbo.Entity
    add UpId nvarchar(10)
go

create index Entity_UpId_index
    on dbo.Entity (UpId)
    go


alter table dbo.Cardholder
    add UpId nvarchar(10)
go

create index Cardholder_UpId_index
    on dbo.Cardholder (UpId)
    go


alter table dbo.CardholderMembership
    add UpId nvarchar(10)
go

create index CardholderMembership_UpId_index
    on dbo.CardholderMembership (UpId)
    go

alter table dbo.CustomFieldValue
    add UpId nvarchar(10)
go

create index CustomFieldValue_UpId_index
    on dbo.CustomFieldValue (UpId)
    go
```

### UP / PeopleSoft database
Scaffolding:
```shell
dotnet ef dbcontext scaffold "Server=10.80.0.4;Database=SAPRO;TrustServerCertificate=True;Integrated Security=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models -t PS_UP_CS_ID_PROGDT -t PS_UP_CS_ID_PROGVW -t PS_UP_CS_SI_UPAGS -t PS_UP_CS_SI_UPGDL -t PS_UP_ID_GRAL_E_VW -t PS_UP_ID_GRAL_VW -t PS_UP_PERSONAL_MD1 -t PS_UP_PERSONAL_MOD -t PS_UP_RH_EMPLS -t PS_UP_RH_EMPLS_DT -t PS_UP_RH_ID_DEPTVW -t PS_UP_RH_ID_DEPTDT --context AppDbContext --data-annotations --nullable --force
```

### Anthology + SAP database
Scaffolding:
```shell
dotnet ef dbcontext scaffold "Server=172.25.3.64;Database=AnthologySync;TrustServerCertificate=True;Integrated Security=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models -t v_UsuariosUnificados --context AppDbContext --data-annotations --nullable --force
```

### Genetec database
Scaffolding:
```shell
cd "C:\Users\dproveedoralusa\RiderProjects\GenetecBridge\Genetec.Data"
dotnet ef dbcontext scaffold "Server=172.25.15.123\ACCESOS;Database=Directory1;TrustServerCertificate=True;User ID=genetec;Password=genetec" Microsoft.EntityFrameworkCore.SqlServer -o Models -t Entity -t Cardholder -t CardholderMembership -t CustomFieldValue -t FileCache -t AlusaControl --context GenetecDbContext --data-annotations --nullable --force
```

#### Analysis for size 8
```sql
;WITH U AS (
    SELECT
        u.EMPLID,
        NormalizedEMPLID = RIGHT(u.EMPLID, 7),
        LenId = LEN(u.EMPLID),
        FirstNameRaw        = LTRIM(RTRIM(u.FIRST_NAME)),
        LastNameRaw         = LTRIM(RTRIM(u.LAST_NAME)),
        SecondLastNameRaw   = LTRIM(RTRIM(u.SECOND_LAST_NAME)),
        FirstNameNorm = UPPER(LTRIM(RTRIM(u.FIRST_NAME))) COLLATE Latin1_General_CI_AI
    FROM v_UsuariosUnificados u
    WHERE u.EMPLID IS NOT NULL AND LEN(u.EMPLID) >= 7
),
      Pairs AS (
          SELECT
              a.NormalizedEMPLID,
              a.EMPLID  AS EMPLID_A,
              b.EMPLID  AS EMPLID_B,
              a.LenId   AS LEN_A,
              b.LenId   AS LEN_B,

              -- Nombres y apellidos
              a.FirstNameRaw      AS FIRST_NAME_A,
              a.LastNameRaw       AS LAST_NAME_A,
              a.SecondLastNameRaw AS SECOND_LAST_NAME_A,

              b.FirstNameRaw      AS FIRST_NAME_B,
              b.LastNameRaw       AS LAST_NAME_B,
              b.SecondLastNameRaw AS SECOND_LAST_NAME_B,

              -- Identifica si uno es de 7 y otro de 8 caracteres
              CASE
                  WHEN a.LenId = 8 AND b.LenId = 7 THEN 'A=8, B=7'
                  WHEN a.LenId = 7 AND b.LenId = 8 THEN 'A=7, B=8'
                  WHEN a.LenId = b.LenId THEN CONCAT('Ambos=', a.LenId)
                  ELSE 'Otro caso'
                  END AS TipoComparacion
          FROM U a
                   JOIN U b
                        ON a.NormalizedEMPLID = b.NormalizedEMPLID
                            AND a.EMPLID < b.EMPLID         -- evita duplicar pares
                            AND a.FirstNameNorm <> b.FirstNameNorm  -- solo diferencias por nombre
      )
 SELECT  DISTINCT *
 FROM Pairs
 ORDER BY NormalizedEMPLID, EMPLID_A, EMPLID_B;
```

#### Normalize EmpId with size 8 in Genetec
> This consider the AnthologyData table was updated before syncing.
> *Run this for Cardholder, CustomFieldValue and Entity tables*

```sql
USE Genetec;
BEGIN TRANSACTION;

UPDATE c
SET c.UpId = v.EMPLID
    FROM dbo.Cardholder c
JOIN v_UsuariosUnificados v
ON RIGHT(v.EMPLID, 7) = c.UpId
WHERE LEN(v.EMPLID) = 8
  AND LEN(c.UpId) = 7;

-- Revisa cuántos registros se afectarían
SELECT @@ROWCOUNT AS RegistrosActualizados;

-- Si todo luce correcto:
-- COMMIT TRANSACTION;

-- Si quieres revertir:
-- ROLLBACK TRANSACTION;

```

Run this to update the UpID in genetec's custom value
```sql
BEGIN TRANSACTION;

UPDATE c
SET c.CF30fd60cbf46340be8a4e8076dcdae701 = v.EMPLID
FROM dbo.CustomFieldValue c
         JOIN AnthologyData v
              ON RIGHT(v.EMPLID, 7) = c.CF30fd60cbf46340be8a4e8076dcdae701
WHERE LEN(v.EMPLID) = 8
  AND LEN(c.CF30fd60cbf46340be8a4e8076dcdae701) = 7;

-- Revisa cuántos registros se afectarían
SELECT @@ROWCOUNT AS RegistrosActualizados;

-- Si todo luce correcto:
COMMIT TRANSACTION;

-- Si quieres revertir:
-- ROLLBACK TRANSACTION;
```

## GenetecSyncConsole usage additions
- Export all pictures to default folder:
  - GenetecSyncConsole --export-pictures
- Export all pictures to a specific folder:
  - GenetecSyncConsole --export-pictures=C:\\temp\\pics
  - or: GenetecSyncConsole --export-pictures C:\\temp\\pics
- Export only pictures for records modified in Anthology on a specific date (yyyy-MM-dd):
  - GenetecSyncConsole --export-pictures --date=2025-10-05
  - GenetecSyncConsole --export-pictures C:\\temp\\pics --date 2025-10-05

Notes:
- The --date filter uses AnthologySap.v_UsuariosUnificados LASTUPDDTTM (date part) to decide which UpIds to export.
- UpId trimming follows the sync behavior (last 7 characters when EMPLID length > 7).
