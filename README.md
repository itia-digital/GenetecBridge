
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
values ('0282574A-F92C-4FF7-A443-D464FCFED598', 'IMSS', 'CLAVE');
```


## Database

### UP database
Scaffolding:
```shell
dotnet ef dbcontext scaffold "Server=172.25.15.123\ACCESOS;Database=Directory1;TrustServerCertificate=True;User ID=genetec;Password=genetec" Microsoft.EntityFrameworkCore.SqlServer -o Models -t Entity -t Cardholder -t CardholderMembership --context GenetecDbContext --data-annotations --nullable
```

### Genetec database
Scaffolding:
```shell
dotnet ef dbcontext scaffold "Server=10.80.0.4;Database=SAPRO;TrustServerCertificate=True;Integrated Security=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models -t PS_UP_CS_ID_PROGDT -t PS_UP_CS_ID_PROGVW -t PS_UP_CS_SI_UPAGS -t PS_UP_CS_SI_UPGDL -t PS_UP_ID_GRAL_E_VW -t PS_UP_ID_GRAL_VW -t PS_UP_PERSONAL_MD1 -t PS_UP_PERSONAL_MOD -t PS_UP_RH_EMPLS -t PS_UP_RH_EMPLS_DT -t PS_UP_RH_ID_DEPTVW -t PS_UP_RH_ID_DEPTDT --context AppDbContext --data-annotations --nullable
```
