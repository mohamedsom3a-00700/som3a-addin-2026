namespace Som3a_WPF_UI.Services
{
    public class GenerationProgress
    {
        public string Phase { get; }
        public int Percentage { get; }
        public string StatusMessage { get; }

        public GenerationProgress(string phase, int percentage, string statusMessage)
        {
            Phase = phase;
            Percentage = percentage;
            StatusMessage = statusMessage;
        }
    }
}