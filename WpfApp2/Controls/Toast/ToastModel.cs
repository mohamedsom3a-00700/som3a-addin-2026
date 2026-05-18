namespace Som3a_WPF_UI.Controls.Toast
{
    public enum ToastType
    {
        Success,
        Warning,
        Error,
        Info
    }

    public class ToastModel
    {
        public string Message { get; set; }
        public ToastType Type { get; set; } = ToastType.Info;
        public int DurationMs { get; set; } = 3000;
    }
}