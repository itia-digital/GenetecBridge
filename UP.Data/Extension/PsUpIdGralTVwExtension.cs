using UP.Data.Models;

namespace UP.Data.Extension;

public static class PsUpIdGralTVwExtension
{
    public static bool IsActive(this PsUpIdGralTVw record)
    {
        string[] inActiveStatuses = ["CN", "DC", "DE", "LA", "I"];
        return !inActiveStatuses.Contains(record.StatusField);
    }
}