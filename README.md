


## Database

Scaffolding:
```shell
dotnet ef dbcontext scaffold "Server=10.80.0.4;Database=SAPRO;TrustServerCertificate=True;Integrated Security=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models -t PS_UP_CS_ID_PROGDT -t PS_UP_CS_ID_PROGVW -t PS_UP_CS_SI_UPAGS -t PS_UP_CS_SI_UPGDL -t PS_UP_ID_GRAL_E_VW -t PS_UP_ID_GRAL_VW -t PS_UP_PERSONAL_MD1 -t PS_UP_PERSONAL_MOD -t PS_UP_RH_EMPLS -t PS_UP_RH_EMPLS_DT -t PS_UP_RH_ID_DEPTVW -t PS_UP_RH_ID_DEPTDT --context AppDbContext --data-annotations --nullable
```
