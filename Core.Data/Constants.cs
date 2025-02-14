namespace Core.Data;

public static class Constants
{
    public static byte GenetecDefaultEntityType = 7;
    public static int GenetecDefaultEntityVerion = 23296;
    // staff Entity.Type = 8
    public static Guid GenetecEmployeeGroup = Guid.Parse("ae1c697e-1049-4b86-ad2f-0ba8dfaaf2ee");
    public static Guid GenetecProfessorGroup = Guid.Parse("607c0999-7891-4734-be30-7226a018ce9a");
    public static Guid GenetecRetiredGroup = Guid.Parse("c9c54c2d-1440-41ac-9a7c-df386c1bb607");
    // students
    public static Guid GenetecStudentGroup = Guid.Parse("e002dde0-de2b-4ca0-acfc-ccde29455310");
    public static Guid GenetecGraduatedGroup = Guid.Parse("f027f4f8-3660-46e4-9bc9-57b2b6f07b2d");
    /*
    ae1c697e-1049-4b86-ad2f-0ba8dfaaf2ee    Empleados de planta
    f027f4f8-3660-46e4-9bc9-57b2b6f07b2d    Alumni (egresados)
    607c0999-7891-4734-be30-7226a018ce9a    Profesores por asignatura
    e002dde0-de2b-4ca0-acfc-ccde29455310    Alumnos
    c9c54c2d-1440-41ac-9a7c-df386c1bb607    Jubilados
     */
    
}