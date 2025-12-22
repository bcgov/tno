using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;

namespace TNO.API.Areas.Helpers;

/// <summary>
/// WatchSubscriptionChange class, provides a way to watch changes that occur to user subscriptions.
/// Currently we are seeing users lose their subscription to reports without an Admin making the change.
/// </summary>
public class WatchSubscriptionChange
{
    #region Variables
    private readonly ILogger _logger;
    private readonly IUserService _userService;
    private readonly IReportService _reportService;
    private readonly IProductService _productService;
    private readonly INotificationService _notificationService;
    private readonly IChesService _chesService;
    private readonly Ches.Configuration.ChesOptions _chesOptions;

    /// <summary>
    /// SubscriptionChange enum, identifies the type of change.
    /// </summary>
    [Flags]
    public enum SubscriptionChangeType
    {
        /// <summary>
        /// The report subscription was removed.
        /// </summary>
        ReportRemoved,
        /// <summary>
        /// The user is no longer subscribed.
        /// </summary>
        ReportUnsubscribed,
        /// <summary>
        /// The product subscription was removed.
        /// </summary>
        ProductRemoved,
        /// <summary>
        /// The notification subscription was removed.
        /// </summary>
        NotificationRemoved,
        /// <summary>
        /// The user is no longer subscribed.
        /// </summary>
        NotificationUnsubscribed
    }

    /// <summary>
    /// SubscriptionChange struct, to provide details about the detected change.
    /// </summary>
    public struct SubscriptionChange
    {
        /// <summary>
        /// get/set - The change type.
        /// </summary>
        public SubscriptionChangeType ChangeType { get; set; }

        /// <summary>
        /// get/set - The user Id.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// get/set - The user email.
        /// </summary>
        public string UserLabel { get; set; }

        /// <summary>
        /// get/set - The primary key to the report/product/notification.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// get/set - The name of the report/product/notification.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Creates a new instance of a SubscriptionChange struct.
        /// </summary>
        /// <param name="changeType"></param>
        /// <param name="userId"></param>
        /// <param name="userLabel"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public SubscriptionChange(SubscriptionChangeType changeType, int userId, string userLabel, int id, string name)
        {
            this.ChangeType = changeType;
            this.UserId = userId;
            this.UserLabel = userLabel;
            this.Id = id;
            this.Name = name;
        }
    }

    /// <summary>
    /// WatchOptions class, provides a way to configure how this watch behaves.
    /// </summary>
    public class WatchOptions
    {
        /// <summary>
        /// get/set - Whether this watch is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// get/set - Comma separated array of email addresses to sent watch alerts to.
        /// </summary>
        public string SendTo { get; set; } = default!;
    }
    #endregion

    #region Properties
    /// <summary>
    /// get/set - The watch options configured.
    /// </summary>
    public WatchOptions Options { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WatchSubscriptionChange class.
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="reportService"></param>
    /// <param name="productService"></param>
    /// <param name="notificationService"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="watchOptions"></param>
    /// <param name="logger"></param>
    public WatchSubscriptionChange(
        IUserService userService,
        IReportService reportService,
        IProductService productService,
        INotificationService notificationService,
        IChesService chesService,
        IOptions<Ches.Configuration.ChesOptions> chesOptions,
        IOptions<WatchOptions> watchOptions,
        ILogger<WatchSubscriptionChange> logger)
    {
        _userService = userService;
        _reportService = reportService;
        _productService = productService;
        _notificationService = notificationService;
        _chesService = chesService;
        _chesOptions = chesOptions.Value;
        this.Options = watchOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generate an email object that identifies all the changes that have occurred to user subscriptions.
    /// </summary>
    /// <param name="changes"></param>
    /// <param name="changeSource">The source of the change, generally the user who made the change.</param>
    /// <param name="traceInformation">The location or other details to help trace the source of the change.</param>
    /// <returns></returns>
    public Ches.Models.EmailModel GenerateEmail(SubscriptionChange[] changes, string changeSource, string traceInformation)
    {
        var body = new StringBuilder($"""
        <div>This is an alert to identify changes to user subscriptions.</div>
        <div>If the changes identified were intentional ignore this email.</div>
        <p style="font-size:30px; line-height:30px; margin:0;">&nbsp;</p>
        <div>{changeSource}</div>
        <h1>Trace Information</h1>
        <div>{traceInformation}</div>
        <h1>Subscription Changes</h1>
        <div><i>Being removed from a product/report/notification may indicate an implementation issue that needs to be resolved.</i></div>
        """);

        var productRemoved = changes.Where(c => c.ChangeType == SubscriptionChangeType.ProductRemoved).ToArray();
        if (productRemoved.Length > 0)
        {
            body.Append($"""
            <h2>Removed from Products</h2>
            <ol style="margin:0; padding:0; margin-left:20px;">
            {String.Join('\n', productRemoved.Select(r => $"<li><div>{r.UserLabel} was removed from ID:{r.Id} \"{r.Name}\"</div></li>").ToArray())}
            </ol>
            """);
        }

        var reportRemoved = changes.Where(c => c.ChangeType == SubscriptionChangeType.ReportRemoved).ToArray();
        if (reportRemoved.Length > 0)
        {
            body.Append($"""
            <h2>Removed from Reports</h2>
            <ol style="margin:0; padding:0; margin-left:20px;">
            {String.Join('\n', reportRemoved.Select(r => $"<li><div>{r.UserLabel} was removed from ID:{r.Id} \"{r.Name}\"</div></li>").ToArray())}
            </ol>
            """);
        }

        var reportUnsubscribed = changes.Where(c => c.ChangeType == SubscriptionChangeType.ReportUnsubscribed).ToArray();
        if (reportUnsubscribed.Length > 0)
        {
            body.Append($"""
            <h2>Unsubscribed from Reports</h2>
            <ol style="margin:0; padding:0; margin-left:20px;">
            {String.Join('\n', reportUnsubscribed.Select(r => $"<li><div>{r.UserLabel} was unsubscribed from ID:{r.Id} \"{r.Name}\"</div></li>").ToArray())}
            </ol>
            """);
        }

        var notificationRemoved = changes.Where(c => c.ChangeType == SubscriptionChangeType.NotificationRemoved).ToArray();
        if (notificationRemoved.Length > 0)
        {
            body.Append($"""
            <h2>Removed from Notifications</h2>
            <ol style="margin:0; padding:0; margin-left:20px;">
            {String.Join('\n', notificationRemoved.Select(r => $"<li><div>{r.UserLabel} was removed from ID:{r.Id} \"{r.Name}\"</div></li>").ToArray())}
            </ol>
            """);
        }

        var notificationUnsubscribed = changes.Where(c => c.ChangeType == SubscriptionChangeType.NotificationUnsubscribed).ToArray();
        if (notificationUnsubscribed.Length > 0)
        {
            body.Append($"""
            <h2>Unsubscribed from Notifications</h2>
            <ol style="margin:0; padding:0; margin-left:20px;">
            {String.Join('\n', notificationUnsubscribed.Select(r => $"<li><div>{r.UserLabel} was unsubscribed from ID:{r.Id} \"{r.Name}\"</div></li>").ToArray())}
            </ol>
            """);
        }

        var to = this.Options.SendTo.Split(',');
        return new Ches.Models.EmailModel(_chesOptions.From, to, "MMI - User Subscription Change Alert", body.ToString());
    }

    #region Users
    /// <summary>
    /// Compare the specified 'user' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="identity"></param>
    /// <param name="traceInformation"></param>
    /// <param name="ignore"></param>
    /// <returns></returns>
    public async Task AlertUserSubscriptionChangedAsync(Entities.User user, ClaimsPrincipal identity, string traceInformation, SubscriptionChangeType? ignore = null)
    {
        var username = identity.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var requestor = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        await AlertUserSubscriptionChangedAsync(user, $"The user who made these changes: {requestor.Email} {requestor.FirstName} {requestor.LastName}", traceInformation, ignore);
    }

    /// <summary>
    /// Compare the specified 'user' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="requestor"></param>
    /// <param name="traceInformation"></param>
    /// <param name="ignore"></param>
    /// <returns></returns>
    public async Task AlertUserSubscriptionChangedAsync(Entities.User user, Entities.User requestor, string traceInformation, SubscriptionChangeType? ignore = null)
    {
        await AlertUserSubscriptionChangedAsync(user, $"The user who made these changes: {requestor.Email} {requestor.FirstName} {requestor.LastName}", traceInformation, ignore);
    }

    /// <summary>
    /// Compare the specified 'user' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="changeSource">The source of the change, generally the user who made the change.</param>
    /// <param name="traceInformation"></param>
    /// <param name="ignore"></param>
    /// <returns></returns>
    public async Task AlertUserSubscriptionChangedAsync(Entities.User user, string changeSource, string traceInformation, SubscriptionChangeType? ignore = null)
    {
        if (!this.Options.IsEnabled) return;

        var changes = IdentifyUserSubscriptionChange(user, ignore);
        if (changes.Length > 0)
        {
            var email = GenerateEmail(changes, changeSource, traceInformation);
            await _chesService.SendEmailAsync(email);
        }
    }
    #endregion

    #region Products
    /// <summary>
    /// Compare the specified 'product' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="product"></param>
    /// <param name="identity"></param>
    /// <param name="traceInformation"></param>
    /// <returns></returns>
    public async Task AlertProductSubscriptionChangedAsync(Entities.Product product, ClaimsPrincipal identity, string traceInformation)
    {
        var username = identity.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var requestor = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        await AlertProductSubscriptionChangedAsync(product, $"The user who made these changes: {requestor.Email} {requestor.FirstName} {requestor.LastName}", traceInformation);
    }

    /// <summary>
    /// Compare the specified 'product' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="product"></param>
    /// <param name="requestor"></param>
    /// <param name="traceInformation"></param>
    /// <returns></returns>
    public async Task AlertProductSubscriptionChangedAsync(Entities.Product product, Entities.User requestor, string traceInformation)
    {
        await AlertProductSubscriptionChangedAsync(product, $"The user who made these changes: {requestor.Email} {requestor.FirstName} {requestor.LastName}", traceInformation);
    }

    /// <summary>
    /// Compare the specified 'product' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="product"></param>
    /// <param name="changeSource">The source of the change, generally the user who made the change.</param>
    /// <param name="traceInformation"></param>
    /// <returns></returns>
    public async Task AlertProductSubscriptionChangedAsync(Entities.Product product, string changeSource, string traceInformation)
    {
        if (!this.Options.IsEnabled) return;

        var changes = IdentifyProductSubscriptionChange(product);
        if (changes.Length > 0)
        {
            var email = GenerateEmail(changes, changeSource, traceInformation);
            await _chesService.SendEmailAsync(email);
        }
    }
    #endregion

    #region Reports
    /// <summary>
    /// Compare the specified 'report' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="identity"></param>
    /// <param name="traceInformation"></param>
    /// <returns></returns>
    public async Task AlertReportSubscriptionChangedAsync(Entities.Report report, ClaimsPrincipal identity, string traceInformation)
    {
        var username = identity.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var requestor = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        await AlertReportSubscriptionChangedAsync(report, $"The user who made these changes: {requestor.Email} {requestor.FirstName} {requestor.LastName}", traceInformation);
    }

    /// <summary>
    /// Compare the specified 'report' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="requestor"></param>
    /// <param name="traceInformation"></param>
    /// <returns></returns>
    public async Task AlertReportSubscriptionChangedAsync(Entities.Report report, Entities.User requestor, string traceInformation)
    {
        await AlertReportSubscriptionChangedAsync(report, $"The user who made these changes: {requestor.Email} {requestor.FirstName} {requestor.LastName}", traceInformation);
    }

    /// <summary>
    /// Compare the specified 'report' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="changeSource">The source of the change, generally the user who made the change.</param>
    /// <param name="traceInformation"></param>
    /// <returns></returns>
    public async Task AlertReportSubscriptionChangedAsync(Entities.Report report, string changeSource, string traceInformation)
    {
        if (!this.Options.IsEnabled) return;

        var changes = IdentifyReportSubscriptionChange(report);
        if (changes.Length > 0)
        {
            var email = GenerateEmail(changes, changeSource, traceInformation);
            await _chesService.SendEmailAsync(email);
        }
    }
    #endregion

    #region Notifications
    /// <summary>
    /// Compare the specified 'notification' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="identity"></param>
    /// <param name="traceInformation"></param>
    /// <returns></returns>
    public async Task AlertNotificationSubscriptionChangedAsync(Entities.Notification notification, ClaimsPrincipal identity, string traceInformation)
    {
        var username = identity.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var requestor = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        await AlertNotificationSubscriptionChangedAsync(notification, $"The user who made these changes: {requestor.Email} {requestor.FirstName} {requestor.LastName}", traceInformation);
    }

    /// <summary>
    /// Compare the specified 'notification' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="requestor"></param>
    /// <param name="traceInformation"></param>
    /// <returns></returns>
    public async Task AlertNotificationSubscriptionChangedAsync(Entities.Notification notification, Entities.User requestor, string traceInformation)
    {
        await AlertNotificationSubscriptionChangedAsync(notification, $"The user who made these changes: {requestor.Email} {requestor.FirstName} {requestor.LastName}", traceInformation);
    }

    /// <summary>
    /// Compare the specified 'notification' subscription information to determine if they have made any subscription changes.
    /// If there are any changes, send an email out if configured to do so.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="changeSource">The source of the change, generally the user who made the change.</param>
    /// <param name="traceInformation"></param>
    /// <returns></returns>
    public async Task AlertNotificationSubscriptionChangedAsync(Entities.Notification notification, string changeSource, string traceInformation)
    {
        if (!this.Options.IsEnabled) return;

        var changes = IdentifyNotificationSubscriptionChange(notification);
        if (changes.Length > 0)
        {
            var email = GenerateEmail(changes, changeSource, traceInformation);
            await _chesService.SendEmailAsync(email);
        }
    }
    #endregion

    /// <summary>
    /// Identify all changes to the specified 'user' subscriptions.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ignore"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public SubscriptionChange[] IdentifyUserSubscriptionChange(Entities.User user, SubscriptionChangeType? ignore = null)
    {
        var currentUser = _userService.FindById(
            user.Id,
            q => q
                .Include(m => m.ReportSubscriptionsManyToMany)
                .Include(m => m.ProductSubscriptionsManyToMany)
                .Include(m => m.NotificationSubscriptionsManyToMany)
        ) ?? throw new InvalidOperationException($"User ID:{user.Id} does not exist.");

        var changes = new List<SubscriptionChange>();

        // Check if product subscriptions changed.
        if (ignore?.HasFlag(SubscriptionChangeType.ProductRemoved) != true)
        {
            foreach (var sub in currentUser.ProductSubscriptionsManyToMany)
            {
                var found = user.ProductSubscriptionsManyToMany.FirstOrDefault(r => r.ProductId == sub.ProductId);
                if (found == null)
                {
                    _logger.LogWarning("User product subscription removed.  User Email: {Email} ProductId: {ProductId}", currentUser.Email, sub.ProductId);
                    changes.Add(new SubscriptionChange(
                        SubscriptionChangeType.ProductRemoved,
                        currentUser.Id,
                        String.IsNullOrWhiteSpace(currentUser.Email) ? currentUser.Username : currentUser.Email,
                        sub.ProductId,
                        ""));
                }
            }
        }

        if (ignore?.HasFlag(SubscriptionChangeType.ReportRemoved) != true || ignore?.HasFlag(SubscriptionChangeType.ReportUnsubscribed) != true)
        {
            // Check if report subscriptions changed.
            foreach (var sub in currentUser.ReportSubscriptionsManyToMany)
            {
                var found = user.ReportSubscriptionsManyToMany.FirstOrDefault(r => r.ReportId == sub.ReportId);
                if (found == null)
                {
                    if (ignore?.HasFlag(SubscriptionChangeType.ReportRemoved) != true)
                    {
                        _logger.LogWarning("User report subscription removed.  User Email: {Email} ReportId: {ReportId}", currentUser.Email, sub.ReportId);
                        changes.Add(new SubscriptionChange(
                            SubscriptionChangeType.ReportRemoved,
                            currentUser.Id,
                            String.IsNullOrWhiteSpace(currentUser.Email) ? currentUser.Username : currentUser.Email,
                            sub.ReportId,
                            ""));
                    }
                }
                else if (sub.IsSubscribed && !found.IsSubscribed)
                {
                    if (ignore?.HasFlag(SubscriptionChangeType.ReportUnsubscribed) != true)
                    {
                        _logger.LogWarning("User unsubscribed from report.  User Email: {Email} ReportId: {ReportId}", currentUser.Email, sub.ReportId);
                        changes.Add(new SubscriptionChange(
                            SubscriptionChangeType.ReportUnsubscribed,
                            currentUser.Id,
                            String.IsNullOrWhiteSpace(currentUser.Email) ? currentUser.Username : currentUser.Email,
                            sub.ReportId,
                            ""));
                    }
                }
            }
        }

        if (ignore?.HasFlag(SubscriptionChangeType.NotificationRemoved) != true || ignore?.HasFlag(SubscriptionChangeType.NotificationUnsubscribed) != true)
        {
            // Check if notification subscriptions changed.
            foreach (var sub in currentUser.NotificationSubscriptionsManyToMany)
            {
                var found = user.NotificationSubscriptionsManyToMany.FirstOrDefault(r => r.NotificationId == sub.NotificationId);
                if (found == null)
                {
                    if (ignore?.HasFlag(SubscriptionChangeType.NotificationRemoved) != true)
                    {
                        _logger.LogWarning("User notification subscription removed.  User Email: {Email} NotificationId: {NotificationId}", currentUser.Email, sub.NotificationId);
                        changes.Add(new SubscriptionChange(
                            SubscriptionChangeType.NotificationRemoved,
                            currentUser.Id,
                            String.IsNullOrWhiteSpace(currentUser.Email) ? currentUser.Username : currentUser.Email,
                            sub.NotificationId,
                            ""));
                    }
                }
                else if (sub.IsSubscribed && !found.IsSubscribed)
                {
                    if (ignore?.HasFlag(SubscriptionChangeType.NotificationUnsubscribed) != true)
                    {
                        _logger.LogWarning("User unsubscribed from notification.  User Email: {Email} NotificationId: {NotificationId}", currentUser.Email, sub.NotificationId);
                        changes.Add(new SubscriptionChange(
                            SubscriptionChangeType.NotificationUnsubscribed,
                            currentUser.Id,
                            String.IsNullOrWhiteSpace(currentUser.Email) ? currentUser.Username : currentUser.Email,
                            sub.NotificationId,
                            ""));
                    }
                }
            }
        }

        // Determine which products/reports/notifications need to be loaded to extract their names.
        var reportIds = changes.Where(c => c.ChangeType == SubscriptionChangeType.ReportRemoved || c.ChangeType == SubscriptionChangeType.ReportUnsubscribed).Select(c => c.Id).ToArray();
        var productIds = changes.Where(c => c.ChangeType == SubscriptionChangeType.ProductRemoved).Select(c => c.Id).ToArray();
        var notificationIds = changes.Where(c => c.ChangeType == SubscriptionChangeType.NotificationRemoved || c.ChangeType == SubscriptionChangeType.NotificationUnsubscribed).Select(c => c.Id).ToArray();

        if (productIds.Length > 0)
        {
            // Update the name of products.
            foreach (var productId in productIds)
            {
                var product = _productService.FindById(productId, null);
                if (product == null) continue;

                var removed = changes.FirstOrDefault(c => c.ChangeType == SubscriptionChangeType.ProductRemoved && c.Id == productId);
                removed.Name = product.Name;
            }
        }

        if (reportIds.Length > 0)
        {
            // Update the name of reports.
            foreach (var reportId in reportIds)
            {
                var report = _reportService.FindById(reportId, null);
                if (report == null) continue;

                var removed = changes.FirstOrDefault(c => c.ChangeType == SubscriptionChangeType.ReportRemoved && c.Id == reportId);
                var unsubscribed = changes.FirstOrDefault(c => c.ChangeType == SubscriptionChangeType.ReportUnsubscribed && c.Id == reportId);
                removed.Name = report.Name;
                unsubscribed.Name = report.Name;
            }
        }

        if (notificationIds.Length > 0)
        {
            // Update the name of notifications.
            foreach (var notificationId in notificationIds)
            {
                var notification = _notificationService.FindById(notificationId, null);
                if (notification == null) continue;

                var removed = changes.FirstOrDefault(c => c.ChangeType == SubscriptionChangeType.NotificationRemoved && c.Id == notificationId);
                var unsubscribed = changes.FirstOrDefault(c => c.ChangeType == SubscriptionChangeType.NotificationUnsubscribed && c.Id == notificationId);
                removed.Name = notification.Name;
                unsubscribed.Name = notification.Name;
            }
        }

        return [.. changes];
    }

    /// <summary>
    /// Identify all changes to the specified 'report' subscriptions.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public SubscriptionChange[] IdentifyReportSubscriptionChange(Entities.Report report)
    {
        var currentReport = _reportService.FindById(
            report.Id,
            q => q
                .Include(m => m.SubscribersManyToMany).ThenInclude(m => m.User)
        ) ?? throw new InvalidOperationException($"Report ID:{report.Id} does not exist.");

        var changes = new List<SubscriptionChange>();

        // Check if report subscriptions changed.
        foreach (var sub in currentReport.SubscribersManyToMany)
        {
            var found = report.SubscribersManyToMany.FirstOrDefault(r => r.UserId == sub.UserId);
            if (found == null)
            {
                _logger.LogWarning("User report subscription removed.  User Email: {Email} ReportID: {ReportId} {Report} ", sub.User?.Email, sub.ReportId, report.Name);
                changes.Add(new SubscriptionChange(
                    SubscriptionChangeType.ReportRemoved,
                    sub.UserId,
                    String.IsNullOrWhiteSpace(sub.User?.Email) ? sub.User?.Username! : sub.User?.Email!,
                    sub.ReportId,
                    report.Name));
            }
            else if (sub.IsSubscribed && !found.IsSubscribed)
            {
                _logger.LogWarning("User unsubscribed from report.  User Email: {Email} ReportID: {ReportId} {Report}", sub.User?.Email, sub.ReportId, report.Name);
                changes.Add(new SubscriptionChange(
                    SubscriptionChangeType.ReportUnsubscribed,
                    sub.UserId,
                    String.IsNullOrWhiteSpace(sub.User?.Email) ? sub.User?.Username! : sub.User?.Email!,
                    sub.ReportId,
                    report.Name));
            }
        }

        return [.. changes];
    }

    /// <summary>
    /// Identify all changes to the specified 'product' subscriptions.
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public SubscriptionChange[] IdentifyProductSubscriptionChange(Entities.Product product)
    {
        var currentProduct = _productService.FindById(product.Id, true, true) ?? throw new InvalidOperationException($"Product ID:{product.Id} does not exist.");

        var changes = new List<SubscriptionChange>();

        // Check if product subscriptions changed.
        foreach (var sub in currentProduct.SubscribersManyToMany)
        {
            var found = product.SubscribersManyToMany.FirstOrDefault(r => r.UserId == sub.UserId);
            if (found == null)
            {
                _logger.LogWarning("User product subscription removed.  User Email: {Email} ProductId: {ProductId} {Product} ", sub.User?.Email, sub.ProductId, product.Name);
                changes.Add(new SubscriptionChange(
                    SubscriptionChangeType.ProductRemoved,
                    sub.UserId,
                    String.IsNullOrWhiteSpace(sub.User?.Email) ? sub.User?.Username! : sub.User?.Email!,
                    sub.ProductId,
                    product.Name));
            }
            else if (currentProduct.ProductType == Entities.ProductType.Report)
            {
                var currentSub = sub.User?.ReportSubscriptionsManyToMany.FirstOrDefault(s => s.ReportId == currentProduct.TargetProductId);
                var reportSub = product.SubscribersManyToMany.FirstOrDefault(s => s.UserId == sub.UserId)?.User?.ReportSubscriptionsManyToMany.FirstOrDefault(s => s.ReportId == currentProduct.TargetProductId);
                if (currentSub != null && reportSub != null && currentSub.IsSubscribed && !reportSub.IsSubscribed)
                {
                    _logger.LogWarning("User unsubscribed from report.  User Email: {Email} ReportId: {ReportId} {Report} ", sub.User?.Email, currentProduct.TargetProductId, product.Name);
                    changes.Add(new SubscriptionChange(
                        SubscriptionChangeType.ReportUnsubscribed,
                        sub.UserId,
                        String.IsNullOrWhiteSpace(sub.User?.Email) ? sub.User?.Username! : sub.User?.Email!,
                        currentProduct.TargetProductId,
                        product.Name));
                }
            }
            else if (currentProduct.ProductType == Entities.ProductType.Notification)
            {
                var currentSub = sub.User?.NotificationSubscriptionsManyToMany.FirstOrDefault(s => s.NotificationId == currentProduct.TargetProductId);
                var reportSub = product.SubscribersManyToMany.FirstOrDefault(s => s.UserId == sub.UserId)?.User?.NotificationSubscriptionsManyToMany.FirstOrDefault(s => s.NotificationId == currentProduct.TargetProductId);
                if (currentSub != null && reportSub != null && currentSub.IsSubscribed && !reportSub.IsSubscribed)
                {
                    _logger.LogWarning("User unsubscribed from notification.  User Email: {Email} NotificationId: {NotificationId} {Notification} ", sub.User?.Email, currentProduct.TargetProductId, product.Name);
                    changes.Add(new SubscriptionChange(
                        SubscriptionChangeType.NotificationUnsubscribed,
                        sub.UserId,
                        String.IsNullOrWhiteSpace(sub.User?.Email) ? sub.User?.Username! : sub.User?.Email!,
                        currentProduct.TargetProductId,
                        product.Name));
                }
            }
        }

        return [.. changes];
    }

    /// <summary>
    /// Identify all changes to the specified 'notification' subscriptions.
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public SubscriptionChange[] IdentifyNotificationSubscriptionChange(Entities.Notification notification)
    {
        var currentNotification = _notificationService.FindById(
            notification.Id,
            q => q
                .Include(m => m.SubscribersManyToMany).ThenInclude(m => m.User)
        ) ?? throw new InvalidOperationException($"Report ID:{notification.Id} does not exist.");

        var changes = new List<SubscriptionChange>();

        // Check if notification subscriptions changed.
        foreach (var sub in currentNotification.SubscribersManyToMany)
        {
            var found = notification.SubscribersManyToMany.FirstOrDefault(r => r.UserId == sub.UserId);
            if (found == null)
            {
                _logger.LogWarning("User notification subscription removed.  User Email: {Email} NotificationId: {NotificationId} {Notification} ", sub.User?.Email, sub.NotificationId, notification.Name);
                changes.Add(new SubscriptionChange(
                    SubscriptionChangeType.NotificationRemoved,
                    sub.UserId,
                    String.IsNullOrWhiteSpace(sub.User?.Email) ? sub.User?.Username! : sub.User?.Email!,
                    sub.NotificationId,
                    notification.Name));
            }
            else if (sub.IsSubscribed && !found.IsSubscribed)
            {
                _logger.LogWarning("User unsubscribed from notification.  User Email: {Email} NotificationId: {NotificationId} {Notification}", sub.User?.Email, sub.NotificationId, notification.Name);
                changes.Add(new SubscriptionChange(
                    SubscriptionChangeType.NotificationUnsubscribed,
                    sub.UserId,
                    String.IsNullOrWhiteSpace(sub.User?.Email) ? sub.User?.Username! : sub.User?.Email!,
                    sub.NotificationId,
                    notification.Name));
            }
        }

        return [.. changes];
    }
    #endregion
}
