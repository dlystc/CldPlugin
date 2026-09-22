namespace UMP.DlyStc.Plugin.Cld.Shared;

/// <summary>
/// Interface for components that can receive and display Stc (sentence) data.
/// Decouples StcFetchService from concrete component types.
/// </summary>
public interface IStcDataReceiver {
    /// <summary>
    /// Receives fetched Stc data and attempts to display it.
    /// </summary>
    /// <param name="data">The fetched data to display.</param>
    /// <returns>true if the data was accepted and displayed; false if the component rejected it (e.g., too long, matches ignore list).</returns>
    bool PushData(StcData data);
}