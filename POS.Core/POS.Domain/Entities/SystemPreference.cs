using POS.Domain.Interfaces;

namespace POS.Domain.Entities;

/// <summary>
/// System-wide preferences and settings that control UI behavior and application configuration
/// </summary>
public class SystemPreference : IStoreEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Store code for which these preferences apply
    /// </summary>
    public int StoreCode { get; set; }

    /// <summary>
    /// Sidebar idle timeout in seconds (default: 10 seconds)
    /// Controls how long the sidebar stays visible after user interaction stops
    /// </summary>
    public int SidebarIdleTimeoutSeconds { get; set; } = 10;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional UI preferences can be added here in the future:
    /// - Theme preferences (light/dark mode)
    /// - Default dashboard view
    /// - Language preferences
    /// </summary>
}
