using SWP391Web.Domain.ValueObjects;

namespace SWP391Web.Domain.Entities
{
    public class EContractManifest
    {
        public string TemplateCode { get; set; } = default!;
        public int TemplateVersionNo { get; set; }
        public Dictionary<string, object?> Data { get; set; } = new();
        public Dictionary<string, AnchorBox> Anchors { get; set; } = new();
    }
}
