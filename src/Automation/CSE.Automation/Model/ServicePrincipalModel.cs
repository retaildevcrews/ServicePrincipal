
namespace CSE.Automation.Model
{
    // Used name ServicePrincipalModel to disambiguate from Microsoft.Graph.ServicePrincipal
    public class ServicePrincipalModel : GraphModel
    {
        public string AppId { get; set; }

        public string AppDisplayName { get; set; }

        public string DisplayName { get; set; }

        public string Notes { get; set; }
    }
}
