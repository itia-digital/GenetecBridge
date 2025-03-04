namespace Core.Data;

public static class Constants
{
    public static readonly byte GenetecCardHolderEntityType = 7;
    public static byte GenetecCredentialEntityType = 9;

    public static readonly int GenetecDefaultEntityVersion = 23296;

    // staff Entity.Type = 8
    public static Guid GenetecEmployeeGroup =
        Guid.Parse("ae1c697e-1049-4b86-ad2f-0ba8dfaaf2ee");

    public static Guid GenetecProfessorGroup =
        Guid.Parse("607c0999-7891-4734-be30-7226a018ce9a");

    public static Guid GenetecRetiredGroup =
        Guid.Parse("c9c54c2d-1440-41ac-9a7c-df386c1bb607");

    // students
    public static Guid GenetecStudentGroup =
        Guid.Parse("e002dde0-de2b-4ca0-acfc-ccde29455310");

    public static Guid GenetecGraduatedGroup =
        Guid.Parse("f027f4f8-3660-46e4-9bc9-57b2b6f07b2d");
    /*
    ae1c697e-1049-4b86-ad2f-0ba8dfaaf2ee    Empleados de planta
    f027f4f8-3660-46e4-9bc9-57b2b6f07b2d    Alumni (egresados)
    607c0999-7891-4734-be30-7226a018ce9a    Profesores por asignatura
    e002dde0-de2b-4ca0-acfc-ccde29455310    Alumnos
    c9c54c2d-1440-41ac-9a7c-df386c1bb607    Jubilados
     */
    
    public static readonly Guid GenetecPartitionDefault =
        Guid.Parse("00000000-0000-0000-0000-00000000000b");

// TODO: Update partitions IDs when Dierctory
    public static readonly Guid GenetecPartitionCdUp =
        Guid.Parse("82a7209a-bdf9-439b-8fca-58a34b3cfb6d");

    public static readonly Guid GenetecPartitionGdl =
        Guid.Parse("e89cf644-9544-4ab7-a899-71da632cf76d");

    public static readonly Guid GenetecPartitionMixcoac =
        Guid.Parse("95d4732d-8d66-4194-a062-748aaffed823");

    public static readonly Guid GenetecPartitionAgs =
        Guid.Parse("28236839-65dd-4ad7-8d2b-f33348c798a8");
}