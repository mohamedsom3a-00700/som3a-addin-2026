/// <summary>
/// Contract for managing RTL/LTR layout switching across the Shell workspace.
/// Implemented by WpfApp2.Controls.Shell.ShellRTLManager.
/// </summary>
public interface IRTLManager
{
    /// <summary>Whether the current layout is RTL.</summary>
    bool IsRTL { get; }

    /// <summary>Apply RTL or LTR layout to the Shell workspace.</summary>
    /// <param name="isRTL">True for RTL, false for LTR.</param>
    void ApplyLayout(bool isRTL);

    /// <summary>Register a UI element for automatic FlowDirection updates.</summary>
    void RegisterFlowElement(FrameworkElement element, string elementId);

    /// <summary>Unregister a previously registered element.</summary>
    void UnregisterFlowElement(string elementId);
}
