using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Subscriber.Models.Folder;

/// <summary>
/// FolderModel class, provides a model that represents an folder.
/// </summary>
public class FolderModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties

    /// <summary>
    /// get/set - Foreign key to user who owns this report.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The owner of this report.
    /// </summary>
    public UserModel? Owner { get; set; }

    /// <summary>
    /// get/set - Foreign key to a filter to apply to the folder.
    /// </summary>
    public int? FilterId { get; set; }

    /// <summary>
    /// get/set - The filter that populates this folder.
    /// </summary>
    public FilterModel? Filter { get; set; }

    /// <summary>
    /// get/set - Foreign key to a schedule to clean this folder.
    /// </summary>
    public int? ScheduleId { get; set; }

    /// <summary>
    /// get/set - A schedule to clean this folder.
    /// </summary>
    public ScheduleModel? Schedule { get; set; }

    /// <summary>
    /// get/set - The folder settings.
    /// </summary>
    public FolderSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - An array of content in this folder.
    /// </summary>
    public IEnumerable<FolderContentModel> Content { get; set; } = Array.Empty<FolderContentModel>();

    /// <summary>
    /// get - An array of reports that use this filter.
    /// </summary>
    public IEnumerable<ReportModel> Reports { get; set; } = Array.Empty<ReportModel>();

    /// <summary>
    /// get/set - An array of event schedules.
    /// </summary>
    public IEnumerable<FolderScheduleModel> Events { get; set; } = Array.Empty<FolderScheduleModel>();

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an FolderModel.
    /// </summary>
    public FolderModel() { }

    /// <summary>
    /// Creates a new instance of an FolderModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public FolderModel(Entities.Folder entity, JsonSerializerOptions options) : base(entity)
    {
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.Settings = JsonSerializer.Deserialize<FolderSettingsModel>(entity.Settings, options) ?? new();
        this.Content = entity.ContentManyToMany.Select(c => new FolderContentModel(c));
        this.Reports = entity.ReportSections.Where(rs => rs.Report != null).Select(rs => new ReportModel(rs.Report!)).ToArray();
        this.Events = entity.Events.Select(e => new FolderScheduleModel(e));
        if (entity.Filter != null)
        {
            this.FilterId = entity.FilterId;
            this.Filter = new FilterModel(entity.Filter, options);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Folder object.
    /// </summary>
    /// <returns></returns>
    public Entities.Folder ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.Folder)this;
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Folder(FolderModel model)
    {
        var entity = new Entities.Folder(model.Id, model.Name, model.OwnerId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            OwnerId = model.OwnerId,
            SortOrder = model.SortOrder,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0,
            FilterId = model.FilterId
        };

        entity.ContentManyToMany.AddRange(model.Content.Select(c => new Entities.FolderContent(model.Id, c.ContentId, c.SortOrder)));
        entity.Events.AddRange(model.Events.Select(e => (Entities.EventSchedule)e));

        return entity;
    }
    #endregion
}
