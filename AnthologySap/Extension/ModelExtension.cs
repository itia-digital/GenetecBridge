using AnthologySap.Models;

namespace AnthologySap.Extension;

public static class ModelExtension
{
    public static bool IsActive(this VUsuariosUnificado record)
    {
        string[] inActiveStatuses = ["CN", "DC", "DE", "LA", "I"];
        return !inActiveStatuses.Contains(record.StatusField);
    }
}