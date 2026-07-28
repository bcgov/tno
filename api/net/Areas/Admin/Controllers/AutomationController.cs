using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Automation;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// AutomationController class, provides automation profile and run endpoints.
/// Profiles, steps, actions, and runs are persisted in the database.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/automation")]
[Route("api/[area]/automation")]
[Route("v{version:apiVersion}/[area]/automation")]
[Route("[area]/automation")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class AutomationController : ControllerBase
{
    #region Variables
    private readonly IAutomationProfileService _profileService;
    private readonly IAutomationRunService _runService;
    private readonly IAutomationRunResponseService _runResponseService;
    private readonly ILLMService _llmService;
    private readonly IContentService _contentService;
    private readonly IEventScheduleService _eventScheduleService;
    private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
    private readonly System.Text.Json.JsonSerializerOptions _serializerOptions;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly Config.KafkaOptions _kafkaOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationController object.
    /// </summary>
    /// <param name="profileService"></param>
    /// <param name="runService"></param>
    /// <param name="runResponseService"></param>
    /// <param name="llmService"></param>
    /// <param name="contentService"></param>
    /// <param name="httpClientFactory"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    public AutomationController(
        IAutomationProfileService profileService,
        IAutomationRunService runService,
        IAutomationRunResponseService runResponseService,
        ILLMService llmService,
        IContentService contentService,
        IEventScheduleService eventScheduleService,
        System.Net.Http.IHttpClientFactory httpClientFactory,
        IOptions<System.Text.Json.JsonSerializerOptions> serializerOptions,
        IKafkaMessenger kafkaMessenger,
        IOptions<Config.KafkaOptions> kafkaOptions)
    {
        _profileService = profileService;
        _runService = runService;
        _runResponseService = runResponseService;
        _llmService = llmService;
        _contentService = contentService;
        _eventScheduleService = eventScheduleService;
        _httpClientFactory = httpClientFactory;
        _serializerOptions = serializerOptions.Value;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
    }
    #endregion

    #region Profile Endpoints
    /// <summary>
    /// Return all automation profiles.
    /// </summary>
    /// <returns></returns>
    [HttpGet("profiles")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AutomationProfileModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult FindProfiles()
    {
        var profiles = _profileService.FindAll().Select(p => new AutomationProfileModel(p));
        return new JsonResult(profiles);
    }

    /// <summary>
    /// Return a single automation profile.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("profiles/{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationProfileModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult FindProfileById(int id)
    {
        var profile = _profileService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new AutomationProfileModel(profile));
    }

    /// <summary>
    /// Create a new automation profile.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("profiles")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationProfileModel), (int)HttpStatusCode.Created)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult AddProfile([FromBody] AutomationProfileModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name)) throw new BadRequestException("Automation profile name is required.");
        if (_profileService.FindAll().Any(p => p.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"Automation profile '{model.Name}' already exists.");

        model.Id = 0;
        var entity = model.ToEntity();
        _profileService.AddAndSave(entity);

        var result = _profileService.FindById(entity.Id) ?? entity;
        return CreatedAtAction(nameof(FindProfileById), new { id = entity.Id }, new AutomationProfileModel(result));
    }

    /// <summary>
    /// Update an existing automation profile.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("profiles/{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationProfileModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult UpdateProfile(int id, [FromBody] AutomationProfileModel model)
    {
        _ = _profileService.FindById(id) ?? throw new NoContentException();
        if (string.IsNullOrWhiteSpace(model.Name)) throw new BadRequestException("Automation profile name is required.");
        if (_profileService.FindAll().Any(p => p.Id != id && p.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"Automation profile '{model.Name}' already exists.");

        model.Id = id;
        var entity = model.ToEntity();
        _profileService.UpdateAndSave(entity);

        var result = _profileService.FindById(id) ?? entity;
        return new JsonResult(new AutomationProfileModel(result));
    }

    /// <summary>
    /// Delete an automation profile.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("profiles/{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationProfileModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult DeleteProfile(int id)
    {
        var entity = _profileService.FindById(id) ?? throw new NoContentException();
        var model = new AutomationProfileModel(entity);
        _profileService.DeleteAndSave(entity);
        return new JsonResult(model);
    }

    #endregion

    #region Debugging Endpoints
    /// <summary>
    /// Ask the profile's LLM why a specific content item was (or was not) acted upon. The full prompt
    /// combines the user's question, the information from the profile's last successful run for that
    /// content item, and the full content item data. Nothing is persisted.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("profiles/{id}/debug")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationDebugResultModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public async Task<IActionResult> DebugContent(int id, [FromBody] AutomationDebugRequestModel request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
            throw new BadRequestException("A question is required.");

        var profile = _profileService.FindById(id) ?? throw new NoContentException();
        if (!profile.LLMId.HasValue) throw new BadRequestException("This automation profile has no LLM configured.");
        var llm = _llmService.FindById(profile.LLMId.Value) ?? throw new BadRequestException("The profile's LLM could not be found.");

        var conversation = new List<(string Role, string Content)>();
        var prompt = "";
        long? runId = null;
        var isFirstTurn = request.Messages == null || !request.Messages.Any();

        if (isFirstTurn)
        {
            // First turn: compose the content and last-run context as the opening user message.
            var content = _contentService.FindById(request.ContentId) ?? throw new NoContentException();
            var lastRun = _runService.Find(id)
                .Where(r => r.Status == Entities.AutomationRunStatus.Completed)
                .OrderByDescending(r => r.StartedOn)
                .FirstOrDefault();
            runId = lastRun?.Id;
            prompt = BuildDebugPrompt(profile, runId, request, content);
            conversation.Add(("system", DebugSystemPrompt));
            conversation.Add(("user", prompt));
        }
        else
        {
            // Continue the existing conversation, appending the user's follow-up message.
            foreach (var message in request.Messages ?? Array.Empty<AutomationDebugMessageModel>())
                conversation.Add((message.Role, message.Content));
            conversation.Add(("user", PromptToText(request.Question)));
        }

        var answer = await InvokeChatAsync(llm, conversation);
        conversation.Add(("assistant", answer));

        return new JsonResult(new AutomationDebugResultModel
        {
            ContentId = request.ContentId,
            RunId = runId,
            Prompt = prompt,
            Answer = answer,
            Messages = conversation.Select(m => new AutomationDebugMessageModel(m.Role, m.Content)).ToArray(),
        });
    }

    private const string DebugSystemPrompt =
        "You are an assistant that helps an editor debug and improve an automated editorial process " +
        "(an \"automation profile\"). You are given: (1) how the profile works and its full configuration " +
        "(its steps in order, and each action with its confirmation marker and criteria); (2) the outcome " +
        "of the profile's last run for a specific content item (what each action decided and which changes " +
        "were applied); and (3) the full content item.\n\n" +
        "Answer the user's question with a clear, specific explanation. When relevant: identify exactly " +
        "which step and action produced the outcome (for example, why the item was or was not published); " +
        "point out any step or action that failed, was skipped, or did not fire, and why; and suggest " +
        "concrete changes to the profile configuration (for example adjusting an action's criteria or its " +
        "confirmation marker) that would change the outcome. Ground every statement in the provided " +
        "configuration, run information, and content - do not invent steps, actions, or rules that are not listed.";

    /// <summary>
    /// Compose the full debugging prompt from the question, the last run's information for the content
    /// item, and the full content item data.
    /// </summary>
    private string BuildDebugPrompt(Entities.AutomationProfile profile, long? runId, AutomationDebugRequestModel request, Entities.Content content)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(PromptToText(request.Question));
        sb.AppendLine();
        sb.AppendLine($"## Automation profile: {profile.Name}");
        if (!string.IsNullOrWhiteSpace(profile.Description)) sb.AppendLine(PromptToText(profile.Description));
        sb.AppendLine();

        // Describe how the profile works (its steps, actions, and criteria) so the LLM can reason
        // about why the content item was or was not acted upon.
        sb.Append(BuildProfileProcessDescription(profile));

        sb.AppendLine("## Last successful automation run");
        if (runId.HasValue)
        {
            var run = _runService.FindById(runId.Value);
            sb.AppendLine($"Run #{runId} completed {run?.CompletedOn:u}.");
            if (!string.IsNullOrWhiteSpace(run?.Note)) sb.AppendLine($"Outcome: {run!.Note}");

            var (responses, changes) = GetRunRecordsForContent(run, request.ContentId);
            sb.AppendLine();
            sb.AppendLine("Actions the automation evaluated for this content item (empty means the action did not fire):");
            if (responses.Count == 0) sb.AppendLine("- (no records for this content item in the last run)");
            foreach (var r in responses)
                sb.AppendLine($"- [{r.Step}{(string.IsNullOrEmpty(r.Action) ? "" : $" / {r.Action}")}]: {(string.IsNullOrWhiteSpace(r.Response) ? "(no response)" : r.Response.Trim())}");

            sb.AppendLine();
            sb.AppendLine("Changes the automation applied to this content item:");
            if (changes.Count == 0) sb.AppendLine("- (none)");
            foreach (var c in changes) sb.AppendLine($"- {c}");
        }
        else
        {
            sb.AppendLine("(No successful run was found for this profile.)");
        }

        sb.AppendLine();
        sb.AppendLine($"## Content item (id {content.Id})");
        sb.AppendLine(System.Text.Json.JsonSerializer.Serialize(new
        {
            content.Id,
            content.Headline,
            content.Byline,
            Source = content.Source?.Name ?? content.OtherSource,
            content.Section,
            content.Page,
            content.Edition,
            Status = content.Status.ToString(),
            ContentType = content.ContentType.ToString(),
            content.PublishedOn,
            content.Summary,
            Body = PromptToText(content.Body),
        }, _serializerOptions));

        return sb.ToString();
    }

    /// <summary>
    /// Describe how the automation profile processes content: its enabled steps (in order), each
    /// step's instructions, and each enabled action with its confirmation marker and criteria. This
    /// gives the LLM the rules the automation actually applies so it can explain a content item's
    /// outcome.
    /// </summary>
    private static string BuildProfileProcessDescription(Entities.AutomationProfile profile)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## How this automation profile works");
        sb.AppendLine(
            "The automation selects content items with the profile's filter, then runs each item through the " +
            "enabled steps below, in order. A step's target controls when it runs: 'content' runs once per " +
            "content item, while 'start' and 'end' run once per run (before and after the items). For each " +
            "step the automation builds a prompt from the step's instructions plus its actions and sends it " +
            "to the LLM; an action is applied only when the LLM response contains that action's confirmation " +
            "marker (shown in quotes below). Actions perform effects such as updating a field, adding tags or " +
            "sentiment, scoring an item, selecting the top-scored items, publishing, deduplicating, or " +
            "aborting. A 'deduplicate' action that finds a duplicate, or a confirmed abort/stop action, halts " +
            "the remaining actions on that step for that item - so actions ordered after it (including " +
            "publish) do not run. An action marked 'always runs' is applied without a confirmation.");
        sb.AppendLine();
        sb.AppendLine("## Profile configuration");

        var steps = profile.Steps.Where(s => s.IsEnabled).OrderBy(s => s.SortOrder).ToList();
        if (steps.Count == 0) sb.AppendLine("(This profile has no enabled steps.)");
        var stepNumber = 1;
        foreach (var step in steps)
        {
            sb.AppendLine($"### Step {stepNumber++}: \"{step.Name}\" (target: {step.Target})");
            if (step.FilterId.HasValue)
                sb.AppendLine(step.ApplyToAutomationFilter
                    ? "This step only acts on content that matches its own filter."
                    : "This step uses a filter to source or enrich content for the prompt.");
            var instructions = PromptToText(step.Prompt);
            if (!string.IsNullOrWhiteSpace(instructions)) sb.AppendLine($"Instructions: {instructions}");

            var actions = step.Actions.Where(a => a.IsEnabled).OrderBy(a => a.SortOrder).ToList();
            if (actions.Count > 0)
            {
                sb.AppendLine("Actions (applied in this order):");
                foreach (var action in actions)
                {
                    var notes = "";
                    if (action.AutoExecute) notes += " [always runs]";
                    if (action.AbortIfNoConfirmation) notes += " [aborts the step if not confirmed]";
                    if (action.MaxCalls.HasValue) notes += $" [max {action.MaxCalls} per run]";
                    if (!string.IsNullOrWhiteSpace(action.Objective)) notes += $" [objective: {action.Objective}]";
                    var confirmation = PromptToText(action.ConfirmationStatement);
                    sb.Append($"- \"{action.Name}\" ({action.ActionType}){notes}");
                    if (!string.IsNullOrWhiteSpace(confirmation)) sb.Append($": confirmed by \"{confirmation}\"");
                    sb.AppendLine();
                    var criteria = PromptToText(action.Prompt);
                    if (!string.IsNullOrWhiteSpace(criteria)) sb.AppendLine($"  Criteria: {criteria}");
                }
            }
            else sb.AppendLine("(No enabled actions.)");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>
    /// Extract the recorded responses and changes for a content item from a run - preferring the
    /// dedicated response table, falling back to the run's summary JSON (older runs).
    /// </summary>
    private (List<(string Step, string? Action, string Response)> Responses, List<string> Changes) GetRunRecordsForContent(Entities.AutomationRun? run, long contentId)
    {
        var responses = new List<(string, string?, string)>();
        var changes = new List<string>();
        if (run == null) return (responses, changes);

        // Responses: dedicated table first.
        foreach (var r in _runResponseService.FindByRun(run.Id).Where(r => r.ContentId == contentId))
            responses.Add((r.StepName, r.ActionName, r.Response));

        if (string.IsNullOrWhiteSpace(run.Summary)) return (responses, changes);
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(run.Summary);
            var root = doc.RootElement;
            if (responses.Count == 0 && root.TryGetProperty("responses", out var reps) && reps.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var r in reps.EnumerateArray())
                {
                    if (!r.TryGetProperty("contentId", out var cid) || !cid.TryGetInt64(out var v) || v != contentId) continue;
                    responses.Add((
                        r.TryGetProperty("stepName", out var sn) ? sn.GetString() ?? "" : "",
                        r.TryGetProperty("actionName", out var an) ? an.GetString() : null,
                        r.TryGetProperty("response", out var rp) ? rp.GetString() ?? "" : ""));
                }
            }
            if (root.TryGetProperty("changes", out var chs) && chs.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var c in chs.EnumerateArray())
                {
                    if (!c.TryGetProperty("contentId", out var cid) || !cid.TryGetInt64(out var v) || v != contentId) continue;
                    var type = c.TryGetProperty("type", out var t) ? t.GetString() : "";
                    var field = c.TryGetProperty("field", out var f) && f.ValueKind == System.Text.Json.JsonValueKind.String ? f.GetString() : null;
                    var value = c.TryGetProperty("value", out var val) && val.ValueKind == System.Text.Json.JsonValueKind.String ? val.GetString() : null;
                    changes.Add($"{type}{(field == null ? "" : $" {field}")}{(value == null ? "" : $" = {value}")}");
                }
            }
        }
        catch
        {
            // A malformed summary must not break debugging.
        }
        return (responses, changes);
    }

    /// <summary>
    /// Strip HTML tags from a value so the prompt carries readable text rather than markup.
    /// </summary>
    private static string PromptToText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var text = System.Text.RegularExpressions.Regex.Replace(value, "<[^>]+>", " ");
        return System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ")).Trim();
    }

    /// <summary>
    /// Invoke the LLM's chat/responses endpoint with a full conversation (deployment-based LLMs).
    /// Mirrors the automation service's chat invocation and supports multi-turn conversations.
    /// </summary>
    private async Task<string> InvokeChatAsync(Entities.LLM llm, IReadOnlyList<(string Role, string Content)> messages)
    {
        if (llm.ProjectEndpoint == null) throw new BadRequestException($"LLM '{llm.Name}' is missing a project endpoint.");
        if (string.IsNullOrWhiteSpace(llm.DeploymentName)) throw new BadRequestException($"LLM '{llm.Name}' requires a deployment name.");
        if (string.IsNullOrWhiteSpace(llm.ApiKey)) throw new BadRequestException($"LLM '{llm.Name}' requires an API key.");
        if (!string.IsNullOrWhiteSpace(llm.AgentName)) throw new BadRequestException($"LLM '{llm.Name}' uses an agent; debugging requires a deployment-based LLM.");

        var isResponsesApi = llm.ProjectEndpoint.AbsolutePath.Contains("/responses", StringComparison.OrdinalIgnoreCase);
        object requestBody = isResponsesApi
            ? new
            {
                model = llm.DeploymentName,
                input = messages.Select(m => (object)new
                {
                    role = m.Item1,
                    content = new object[] { new { type = m.Item1 == "assistant" ? "output_text" : "input_text", text = m.Item2 } },
                }).ToArray(),
            }
            : new
            {
                model = llm.DeploymentName,
                messages = messages.Select(m => (object)new { role = m.Item1, content = m.Item2 }).ToArray(),
            };

        var requestJson = System.Text.Json.JsonSerializer.Serialize(requestBody, _serializerOptions);
        using var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(5);
        using var httpRequest = new System.Net.Http.HttpRequestMessage(HttpMethod.Post, llm.ProjectEndpoint);
        httpRequest.Headers.Add("api-key", llm.ApiKey);
        httpRequest.Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

        var response = await client.SendAsync(httpRequest);
        var responseJson = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new BadRequestException($"LLM '{llm.Name}' request failed ({(int)response.StatusCode}).");

        if (isResponsesApi) return ParseResponsesOutput(responseJson);
        var data = System.Text.Json.JsonSerializer.Deserialize<TNO.Models.Azure.ChatCompletionResponse>(responseJson);
        return data?.Choices?.FirstOrDefault()?.Message?.Content ?? "";
    }

    /// <summary>
    /// Extract the output text from a Responses API result.
    /// </summary>
    private static string ParseResponsesOutput(string responseJson)
    {
        using var document = System.Text.Json.JsonDocument.Parse(responseJson);
        var root = document.RootElement;
        if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == System.Text.Json.JsonValueKind.String)
            return outputText.GetString() ?? "";
        if (!root.TryGetProperty("output", out var output) || output.ValueKind != System.Text.Json.JsonValueKind.Array)
            return "";
        var text = new System.Text.StringBuilder();
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("type", out var type) || type.GetString() != "message") continue;
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != System.Text.Json.JsonValueKind.Array) continue;
            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("type", out var partType) && partType.GetString() == "output_text" &&
                    part.TryGetProperty("text", out var partText) && partText.ValueKind == System.Text.Json.JsonValueKind.String)
                    text.AppendLine(partText.GetString());
            }
        }
        return text.ToString().Trim();
    }
    #endregion

    #region Run Endpoints
    /// <summary>
    /// Queue an automation run for a profile. The automation service picks up queued runs and executes them.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("profiles/{id}/run")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationRunModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public async Task<IActionResult> RunProfile(int id, [FromBody] AutomationRunRequestModel? request)
    {
        _ = _profileService.FindById(id) ?? throw new NoContentException();

        var run = new Entities.AutomationRun(id, string.IsNullOrWhiteSpace(request?.Trigger) ? "manual" : request!.Trigger!)
        {
            Status = Entities.AutomationRunStatus.Draft,
            Note = request?.Note,
            StartedOn = DateTime.UtcNow,
        };
        _runService.AddAndSave(run);

        // Publish a work item so an automation service instance picks up the run.
        // The queued run remains the source of truth; the service reconciles stale runs if a message is lost.
        await _kafkaMessenger.SendMessageAsync(
            _kafkaOptions.AutomationTopic,
            run.Id.ToString(),
            new AutomationRequestModel(run.Id, id));

        return new JsonResult(new AutomationRunModel(run));
    }

    /// <summary>
    /// Clear the last-run information for the specified schedule so it becomes eligible to run
    /// again (useful for testing). The scheduler uses 'RequestSentOn' to enforce the once-per-day
    /// rule; both 'RequestSentOn' and 'LastRanOn' are cleared.
    /// </summary>
    /// <param name="id">The automation profile id.</param>
    /// <param name="scheduleId">The event schedule id belonging to the profile.</param>
    /// <returns></returns>
    [HttpPost("profiles/{id}/schedules/{scheduleId}/clear-last-run")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationScheduleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult ClearScheduleLastRun(int id, int scheduleId)
    {
        var eventSchedule = _eventScheduleService.FindById(scheduleId);
        if (eventSchedule == null
            || eventSchedule.EventType != Entities.EventScheduleType.Automation
            || eventSchedule.AutomationProfileId != id)
            return new NoContentResult();

        eventSchedule.RequestSentOn = null;
        eventSchedule.LastRanOn = null;
        eventSchedule = _eventScheduleService.UpdateAndSave(eventSchedule);

        return new JsonResult(new AutomationScheduleModel(eventSchedule));
    }

    /// <summary>
    /// Return a single automation run.
    /// </summary>
    /// <param name="runId"></param>
    /// <returns></returns>
    [HttpGet("runs/{runId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationRunModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult FindRunById(long runId)
    {
        var run = _runService.FindById(runId) ?? throw new NoContentException();
        return new JsonResult(new AutomationRunModel(run));
    }

    /// <summary>
    /// Atomically claim a queued run (Draft -> Running). Only one caller wins; used so
    /// horizontally scaled automation service instances never execute the same run.
    /// </summary>
    /// <param name="runId"></param>
    /// <returns>Whether the caller claimed the run.</returns>
    [HttpPost("runs/{runId}/claim")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult ClaimRun(long runId)
    {
        var claimed = _runService.TryClaim(runId);
        return new JsonResult(claimed);
    }

    /// <summary>
    /// Update an automation run (status, note, completion). Used by the automation service to report
    /// progress and outcome. The summary is updated separately via <see cref="UpdateRunSummary"/>.
    /// </summary>
    /// <param name="runId"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("runs/{runId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationRunModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult UpdateRun(long runId, [FromBody] AutomationRunModel model)
    {
        var run = _runService.FindById(runId) ?? throw new NoContentException();
        run.Status = (Entities.AutomationRunStatus)(int)model.Status;
        run.Note = model.Note;
        run.CompletedOn = model.CompletedOn;
        // The summary is written exclusively by UpdateRunSummary (raw body) so it is never
        // re-escaped as a JSON string property; leave the persisted value untouched here.
        _runService.UpdateAndSave(run);
        return new JsonResult(new AutomationRunModel(run));
    }

    /// <summary>
    /// Update only the summary of an automation run. The summary is read directly from the raw
    /// request body rather than bound as a JSON string property, so a very large summary is never
    /// escaped/re-escaped into a single buffer (that path OOMs the automation service and the API).
    /// </summary>
    /// <param name="runId"></param>
    /// <returns></returns>
    [HttpPut("runs/{runId}/summary")]
    [DisableRequestSizeLimit]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public async Task<IActionResult> UpdateRunSummary(long runId)
    {
        var run = _runService.FindById(runId) ?? throw new NoContentException();
        using var reader = new System.IO.StreamReader(Request.Body, System.Text.Encoding.UTF8);
        var summary = await reader.ReadToEndAsync();
        run.Summary = string.IsNullOrWhiteSpace(summary) ? null : summary;
        _runService.UpdateAndSave(run);
        return new NoContentResult();
    }

    /// <summary>
    /// Return automation run history, optionally filtered by profile.
    /// </summary>
    /// <param name="profileId"></param>
    /// <returns></returns>
    [HttpGet("runs")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AutomationRunModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult FindRuns([FromQuery] int? profileId)
    {
        var runs = _runService.Find(profileId).Select(r => new AutomationRunModel(r));
        return new JsonResult(runs);
    }

    /// <summary>
    /// Return run diff payload from the run summary recorded by the automation service.
    /// </summary>
    /// <param name="runId"></param>
    /// <returns></returns>
    [HttpGet("runs/{runId}/diff")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult GetRunDiff(long runId)
    {
        var run = _runService.FindById(runId) ?? throw new NoContentException();

        System.Text.Json.JsonElement? changes = null;
        System.Text.Json.JsonElement? stepHits = null;
        if (!string.IsNullOrWhiteSpace(run.Summary))
        {
            try
            {
                var summary = System.Text.Json.JsonDocument.Parse(run.Summary);
                if (summary.RootElement.TryGetProperty("changes", out var changesElement))
                    changes = changesElement.Clone();
                if (summary.RootElement.TryGetProperty("steps", out var stepsElement))
                    stepHits = stepsElement.Clone();
            }
            catch
            {
                // A malformed summary should not break the endpoint.
            }
        }

        // Prompt/response text is stored separately (not in the summary) to keep run data small.
        var responses = _runResponseService.FindByRun(runId).Select(r => new AutomationRunResponseModel(r));

        return new JsonResult(new
        {
            Run = new AutomationRunModel(run),
            Changes = (object?)changes ?? Array.Empty<object>(),
            StepHits = (object?)stepHits ?? Array.Empty<object>(),
            Responses = responses,
        });
    }

    /// <summary>
    /// Append a batch of LLM prompt/response records to the specified run. The automation service
    /// posts these incrementally (per step) so the large prompt/response text is never accumulated
    /// in the run summary or held in the service's memory for the whole run.
    /// </summary>
    /// <param name="runId"></param>
    /// <param name="responses"></param>
    /// <returns>The number of responses added.</returns>
    [HttpPost("runs/{runId}/responses")]
    [DisableRequestSizeLimit]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult AddRunResponses(long runId, [FromBody] IEnumerable<AutomationRunResponseModel> responses)
    {
        _ = _runService.FindById(runId) ?? throw new NoContentException();
        var added = _runResponseService.AddRange(responses.Select(r => r.ToEntity(runId)));
        return new JsonResult(added);
    }

    /// <summary>
    /// Prune automation runs older than the specified number of days. Used by the automation service for retention.
    /// </summary>
    /// <param name="days"></param>
    /// <returns>The number of runs deleted.</returns>
    [HttpDelete("runs/prune")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult PruneRuns([FromQuery] int days)
    {
        var deleted = _runService.DeleteOlderThan(days);
        return new JsonResult(deleted);
    }
    #endregion
}
