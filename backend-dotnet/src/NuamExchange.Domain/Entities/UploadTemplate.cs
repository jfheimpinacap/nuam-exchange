namespace NuamExchange.Domain.Entities;

public sealed class UploadTemplate
{
    public int Id { get; set; }
    public string UploadType { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RequiredColumns { get; set; } = string.Empty;
    public string AllowedFormat { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = "1.0";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public ICollection<UploadFile> UploadFiles { get; } = new List<UploadFile>();
}
