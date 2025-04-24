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

    public static readonly Guid GenetecPartitionCdUp =
        Guid.Parse("45751766-ad10-4661-a9e6-d4068cd148d5");

    public static readonly Guid GenetecPartitionGdl =
        Guid.Parse("8ac9a9d6-3fd7-453d-80fa-d239ac3feb2b");

    public static readonly Guid GenetecPartitionMixcoac =
        Guid.Parse("b8d88f9c-4de1-4ddd-bcd7-ff56e37443ee");

    public static readonly Guid GenetecPartitionAgs =
        Guid.Parse("2b30852b-c0c0-4717-b76b-d2403a475655");
}