namespace CromulentBisgetti.DemoApp.Models
{
    public sealed class ErrorViewModel
    {
        public string RequestId { get; set; } = string.Empty;

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
