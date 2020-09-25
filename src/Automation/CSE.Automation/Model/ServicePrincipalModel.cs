
namespace CSE.Automation.Model
{
    // Used name ServicePrincipalModel to disambituate from Microsoft.Graph.ServicePrincipal
    public class ServicePrincipalModel : GraphModel
    {
        public string AppId { get; set; }

        public string AppDisplayName { get; set; }

        public string DisplayName { get; set; }

        public string Notes { get; set; }
    }
}
