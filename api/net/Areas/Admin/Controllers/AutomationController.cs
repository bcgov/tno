using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Automation;
using TNO.API.Areas.Admin.Models.Automation.V2;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using SignalRModels = TNO.API.Models.SignalR;

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
    private readonly IAutomationRunLogService _runLogService;
    private readonly ILLMService _llmService;
    private readonly IContentService _contentService;
    private readonly IEventScheduleService _eventScheduleService;
    private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
    private readonly System.Text.Json.JsonSerializerOptions _serializerOptions;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly Config.KafkaOptions _kafkaOptions;
    private readonly KafkaHubConfig _kafkaHubOptions;
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
    /// <param name="eventScheduleService"></param>
    /// <param name="httpClientFactory"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="kafkaHubOptions"></param>
    public AutomationController(
        IAutomationProfileService profileService,
        IAutomationRunService runService,
        IAutomationRunLogService runLogService,
        ILLMService llmService,
        IContentService contentService,
        IEventScheduleService eventScheduleService,
        System.Net.Http.IHttpClientFactory httpClientFactory,
        IOptions<System.Text.Json.JsonSerializerOptions> serializerOptions,
        IKafkaMessenger kafkaMessenger,
        IOptions<Config.KafkaOptions> kafkaOptions,
        IOptions<KafkaHubConfig> kafkaHubOptions)
    {
        _profileService = profileService;
        _runService = runService;
        _runLogService = runLogService;
        _llmService = llmService;
        _contentService = contentService;
        _eventScheduleService = eventScheduleService;
        _httpClientFactory = httpClientFactory;
        _serializerOptions = serializerOptions.Value;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
        _kafkaHubOptions = kafkaHubOptions.Value;
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

        var validationErrors = ValidateDefinition(model);
        if (validationErrors != null) return validationErrors;

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

        var validationErrors = ValidateDefinition(model);
        if (validationErrors != null) return validationErrors;

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
            // First turn: compose the profile, most-recent-run, and (optional) content context as
            // the opening user message. Without a content item the question is answered against
            // the run as a whole.
            var content = request.ContentId > 0
                ? _contentService.FindById(request.ContentId) ?? throw new NoContentException()
                : null;
            // The decision log is written incrementally, so a run still executing is fair game -
            // the prompt notes that its information is partial.
            var lastRun = _runService.Find(id)
                .Where(r => r.Status == Entities.AutomationRunStatus.Completed
                    || r.Status == Entities.AutomationRunStatus.Failed
                    || r.Status == Entities.AutomationRunStatus.Running)
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
    private string BuildDebugPrompt(Entities.AutomationProfile profile, long? runId, AutomationDebugRequestModel request, Entities.Content? content)
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

        sb.AppendLine("## Most recent automation run");
        if (runId.HasValue)
        {
            var run = _runService.FindById(runId.Value);
            if (run?.CompletedOn == null)
                sb.AppendLine($"Run #{runId} is STILL EXECUTING (started {run?.StartedOn:u}); the information below is partial.");
            else
                sb.AppendLine($"Run #{runId} ({run?.Status}) completed {run?.CompletedOn:u}.");
            if (!string.IsNullOrWhiteSpace(run?.Note)) sb.AppendLine($"Outcome: {run!.Note}");

            if (content != null)
            {
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
                // No content item: whole-run outcome counts (so aggregate questions have real
                // numbers), then the decision log tail for detail.
                sb.AppendLine();
                sb.AppendLine("Outcome counts for the WHOLE run, grouped by step (on a Detect Duplicate action, 'confirmed' means a duplicate was found and 'not-confirmed' means no match):");
                foreach (var (stepName, outcome, count) in _runLogService.CountByRun(runId.Value))
                    sb.AppendLine($"- {stepName}: {outcome} = {count}");

                var (_, totalLogs) = _runLogService.FindByRun(runId.Value, qty: 1);
                var lastPage = Math.Max(1, (int)Math.Ceiling(totalLogs / 80.0));
                var (tail, _) = _runLogService.FindByRun(runId.Value, page: lastPage, qty: 80);
                sb.AppendLine();
                sb.AppendLine($"Decision log (most recent {Math.Min(80, totalLogs)} of {totalLogs} entries):");
                foreach (var l in tail)
                {
                    var text = (l.Response ?? "").Trim();
                    if (text.Length > 240) text = text[..240] + "…";
                    sb.AppendLine($"- [{l.StepName}{(string.IsNullOrEmpty(l.ActionName) ? "" : $" / {l.ActionName}")}] {l.Outcome}{(l.ContentId.HasValue ? $" (content {l.ContentId})" : "")}: {text}");
                }
            }
        }
        else
        {
            sb.AppendLine("(No completed run was found for this profile.)");
        }

        sb.AppendLine();
        if (content != null)
        {
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
        }
        else
        {
            sb.AppendLine("(No specific content item was selected; answer about the run as a whole.)");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Describe how the automation profile processes content from its definition document: the
    /// enabled steps by phase, each step's analyses, and each enabled action with its gate. This
    /// gives the LLM the rules the automation actually applies so it can explain an outcome.
    /// </summary>
    private static string BuildProfileProcessDescription(Entities.AutomationProfile profile)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## How this automation profile works");
        sb.AppendLine(
            "The profile is a definition document. Steps run by phase: 'init' steps run once (typically " +
            "searching content into named collections), 'process' steps run once per item of their source " +
            "collection, and 'complete' steps run after all items. Each step declares analyses (LLM prompts " +
            "producing named results, sent lazily when first used) and ordered actions. An action runs when " +
            "its gate passes: always, a condition over analysis results and content fields, or an LLM " +
            "confirmation statement. Changes accumulate on working copies and are only written by a " +
            "Save Collection or Save Content Now action.");
        sb.AppendLine();
        sb.AppendLine("## Profile configuration");

        var definitionJson = profile.Definition?.RootElement.GetRawText();
        if (string.IsNullOrWhiteSpace(definitionJson))
        {
            sb.AppendLine("(This profile has no definition document.)");
            return sb.ToString();
        }

        try
        {
            var definition = AutomationDefinition.Parse(definitionJson);
            var steps = definition.Steps.Where(s => s.IsEnabled).ToList();
            if (steps.Count == 0) sb.AppendLine("(This profile has no enabled steps.)");
            var stepNumber = 1;
            foreach (var step in steps)
            {
                sb.AppendLine($"### Step {stepNumber++}: \"{step.Name}\" (phase: {step.Phase})");
                if (step.Source?.Collection != null)
                    sb.AppendLine($"Iterates the '{step.Source.Collection}' collection.");
                foreach (var analysis in step.Analyses)
                {
                    var promptRef = !string.IsNullOrWhiteSpace(analysis.Prompt.Ref)
                        ? $"library prompt '{analysis.Prompt.Ref}'"
                        : "an inline prompt";
                    var returns = analysis.Returns.Count > 0
                        ? $" returning {string.Join(", ", analysis.Returns.Select(r => $"{r.Key} ({r.Value})"))}"
                        : analysis.Raw ? " returning the raw response" : "";
                    sb.AppendLine($"- Analysis \"{analysis.Name}\" uses {promptRef}{returns}.");
                }
                var actions = step.Actions.Where(a => a.IsEnabled).ToList();
                if (actions.Count > 0)
                {
                    sb.AppendLine("Actions (applied in this order):");
                    foreach (var action in actions)
                    {
                        var gate = !string.IsNullOrWhiteSpace(action.Confirm)
                            ? $" [runs when the LLM responds \"{action.Confirm}\"{(string.IsNullOrWhiteSpace(action.Analysis) ? "" : $" from analysis '{action.Analysis}'")}]"
                            : action.When != null ? " [runs when its condition passes]" : " [always runs]";
                        sb.AppendLine($"- \"{action.Name ?? action.Type}\" ({action.Type}){gate}");
                    }
                }
                else sb.AppendLine("(No enabled actions.)");
                sb.AppendLine();
            }
        }
        catch (System.Text.Json.JsonException)
        {
            sb.AppendLine("(The definition document could not be parsed.)");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Extract the recorded responses and changes for a content item from a run: the per-item
    /// trace from the decision log, plus the changes recorded in the run's summary JSON.
    /// </summary>
    private (List<(string Step, string? Action, string Response)> Responses, List<string> Changes) GetRunRecordsForContent(Entities.AutomationRun? run, long contentId)
    {
        var responses = new List<(string, string?, string)>();
        var changes = new List<string>();
        if (run == null) return (responses, changes);

        // The per-item trace lives in the decision log.
        {
            var (logs, _) = _runLogService.FindByRun(run.Id, contentId: contentId, qty: 300);
            foreach (var l in logs)
            {
                var text = (l.Response ?? "").Trim();
                if (text.Length > 400) text = text[..400] + "…";
                responses.Add((l.StepName, l.ActionName ?? l.AnalysisName, $"{l.Outcome}{(text.Length > 0 ? $": {text}" : "")}"));
            }
        }

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
                    // v1 summaries carry 'contentId' (number); v2 summaries 'contentRef' (string).
                    var matches = c.TryGetProperty("contentId", out var cid) && cid.TryGetInt64(out var v) && v == contentId;
                    if (!matches && c.TryGetProperty("contentRef", out var cref)
                        && cref.ValueKind == System.Text.Json.JsonValueKind.String
                        && cref.GetString() == contentId.ToString())
                        matches = true;
                    if (!matches) continue;
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

        // A comparison run always executes dry - it exists to show differences, not to act twice.
        System.Text.Json.JsonDocument? compareDefinition = null;
        if (!string.IsNullOrWhiteSpace(request?.CompareDefinition))
        {
            try
            {
                var candidate = AutomationDefinition.Parse(request!.CompareDefinition!);
                var candidateErrors = AutomationDefinitionValidator.Validate(candidate).Where(e => e.Severity == "error").ToArray();
                if (candidateErrors.Length > 0) return BadRequest(new { errors = candidateErrors });
                compareDefinition = System.Text.Json.JsonDocument.Parse(request.CompareDefinition!);
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new BadRequestException($"The comparison definition is not valid JSON: {ex.Message}");
            }
        }

        var run = new Entities.AutomationRun(id, string.IsNullOrWhiteSpace(request?.Trigger) ? "manual" : request!.Trigger!)
        {
            Status = Entities.AutomationRunStatus.Draft,
            Note = request?.Note,
            StartedOn = DateTime.UtcNow,
            IsDryRun = request?.IsDryRun == true || compareDefinition != null,
            CompareDefinition = compareDefinition,
        };
        _runService.AddAndSave(run);

        // Notify editors (via SignalR) that a run has begun so a scheduled run appears without a
        // page refresh - the run may have been queued by the scheduler, not this user.
        await _kafkaMessenger.SendMessageAsync(
            _kafkaHubOptions.HubTopic,
            new KafkaHubMessage(HubEvent.SendAll,
                new KafkaInvocationMessage(MessageTarget.AutomationRunUpdated,
                    new[] { new SignalRModels.AutomationRunMessageModel(run) })));

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
    public async Task<IActionResult> UpdateRun(long runId, [FromBody] AutomationRunModel model)
    {
        var run = _runService.FindById(runId) ?? throw new NoContentException();
        run.Status = (Entities.AutomationRunStatus)(int)model.Status;
        run.Note = model.Note;
        run.CompletedOn = model.CompletedOn;
        // The summary is written exclusively by UpdateRunSummary (raw body) so it is never
        // re-escaped as a JSON string property; leave the persisted value untouched here.
        _runService.UpdateAndSave(run);

        // Notify editors (via SignalR) of the status change so the run's progress updates live.
        await _kafkaMessenger.SendMessageAsync(
            _kafkaHubOptions.HubTopic,
            new KafkaHubMessage(HubEvent.SendAll,
                new KafkaInvocationMessage(MessageTarget.AutomationRunUpdated,
                    new[] { new SignalRModels.AutomationRunMessageModel(run) })));

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
    // Bounded (not unlimited): summaries are engine-truncated; 10MB is generous headroom while
    // preventing a runaway payload from ballooning API memory.
    [RequestSizeLimit(10_485_760)]
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

        return new JsonResult(new
        {
            Run = new AutomationRunModel(run),
            Changes = (object?)changes ?? Array.Empty<object>(),
            StepHits = (object?)stepHits ?? Array.Empty<object>(),
        });
    }

    /// <summary>
    /// Delete a single automation run from the run history. The run's responses are removed with it
    /// (cascade). This only removes history - it does not affect scheduling, which is driven by the
    /// schedule's own last-run date rather than by the run records.
    /// </summary>
    /// <param name="runId"></param>
    /// <returns>The deleted run.</returns>
    // The 'long' constraint keeps this from capturing the literal "runs/prune" route below.
    [HttpDelete("runs/{runId:long}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationRunModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult DeleteRun(long runId)
    {
        var run = _runService.FindById(runId) ?? throw new NoContentException();
        var model = new AutomationRunModel(run);
        _runService.DeleteAndSave(run);
        return new JsonResult(model);
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

    #region V2 Endpoints
    /// <summary>
    /// Return the v2 action catalog: every registered action type with its descriptor
    /// (phases, requirements, and configuration fields). The editor renders action forms from
    /// these descriptors so it follows the engine automatically.
    /// </summary>
    /// <returns></returns>
    [HttpGet("v2/descriptors")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<V2ActionDescriptor>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult GetV2Descriptors()
    {
        return new JsonResult(V2ActionCatalog.Types.Values.OrderBy(d => d.Category).ThenBy(d => d.Label));
    }

    /// <summary>
    /// Validate a v2 profile definition without saving it. Returns every finding (errors and
    /// warnings) with the definition path it anchors to.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("profiles/validate")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<V2ValidationError>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult ValidateProfile([FromBody] AutomationProfileModel model)
    {
        if (model.SchemaVersion < 2 || string.IsNullOrWhiteSpace(model.Definition))
            return new JsonResult(Array.Empty<V2ValidationError>());
        try
        {
            var definition = AutomationDefinition.Parse(model.Definition!);
            return new JsonResult(AutomationDefinitionValidator.Validate(definition));
        }
        catch (System.Text.Json.JsonException ex)
        {
            return new JsonResult(new[] { new V2ValidationError("definition", $"The definition is not valid JSON: {ex.Message}") });
        }
    }

    /// <summary>
    /// Return a page of the run's decision log, in execution order, with optional filters.
    /// Every prompt and response is recorded (no capture flag); entries are retained for the
    /// current date only.
    /// </summary>
    /// <param name="runId"></param>
    /// <param name="step"></param>
    /// <param name="action"></param>
    /// <param name="outcome"></param>
    /// <param name="contentId"></param>
    /// <param name="search"></param>
    /// <param name="page"></param>
    /// <param name="qty"></param>
    /// <returns></returns>
    [HttpGet("runs/{runId}/logs")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult FindRunLogs(long runId, [FromQuery] string? step, [FromQuery] string? action, [FromQuery] string? outcome, [FromQuery] long? contentId, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int qty = 100, [FromQuery] string? direction = null)
    {
        _ = _runService.FindById(runId) ?? throw new NoContentException();
        var (items, total) = _runLogService.FindByRun(runId, step, action, outcome, contentId, search, page, qty,
            string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase));
        return new JsonResult(new
        {
            items = items.Select(l => new AutomationRunLogModel(l)),
            page,
            qty,
            total,
        });
    }

    /// <summary>
    /// Append a batch of decision log entries to the specified run. Used by the automation service,
    /// which flushes its log buffer incrementally so a failed run still has its log up to the failure.
    /// </summary>
    /// <param name="runId"></param>
    /// <param name="logs"></param>
    /// <returns></returns>
    [HttpPost("runs/{runId}/logs")]
    [DisableRequestSizeLimit]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult AddRunLogs(long runId, [FromBody] IEnumerable<AutomationRunLogModel> logs)
    {
        _ = _runService.FindById(runId) ?? throw new NoContentException();
        var added = _runLogService.AddRange(logs.Select(l => l.ToEntity(runId)));
        return new JsonResult(added);
    }

    /// <summary>
    /// Delete decision log entries created before the specified cutoff (UTC). Used by the
    /// automation service's daily sweep - the log retention (current date) is independent of the
    /// run-history retention.
    /// </summary>
    /// <param name="cutoff"></param>
    /// <returns>The number of entries deleted.</returns>
    [HttpDelete("runs/logs/prune")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public IActionResult PruneRunLogs([FromQuery] DateTime cutoff)
    {
        if (cutoff == default) throw new BadRequestException("A cutoff date is required.");
        var deleted = _runLogService.Prune(cutoff.ToUniversalTime());
        return new JsonResult(deleted);
    }

    /// <summary>
    /// Open (or continue) an explain-and-improve conversation about one run log entry. The
    /// conversation is seeded with the entry's exact prompt, response, parsed outcome, action
    /// configuration, and content reference. When the assistant proposes a prompt revision it is
    /// returned in SuggestedPrompt for the editor to show as a diff - nothing is applied
    /// automatically, and the conversation itself is logged and attributed to the caller.
    /// </summary>
    /// <param name="runId"></param>
    /// <param name="logId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("runs/{runId}/logs/{logId}/explain")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AutomationExplainResultModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Automation" })]
    public async Task<IActionResult> ExplainRunLog(long runId, long logId, [FromBody] AutomationExplainRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Question)) throw new BadRequestException("A question is required.");
        var run = _runService.FindById(runId) ?? throw new NoContentException();
        var entry = _runLogService.FindById(logId);
        if (entry == null || entry.AutomationRunId != runId) throw new NoContentException();

        var profile = _profileService.FindById(run.AutomationProfileId) ?? throw new NoContentException();
        if (!profile.LLMId.HasValue) throw new BadRequestException("This automation profile has no LLM configured.");
        var llm = _llmService.FindById(profile.LLMId.Value) ?? throw new BadRequestException("The profile's LLM could not be found.");

        var conversation = new List<(string Role, string Content)>();
        if (!request.Messages.Any())
        {
            conversation.Add(("system", ExplainSystemPrompt));
            conversation.Add(("user", BuildExplainPrompt(profile, entry, request.Question)));
        }
        else
        {
            foreach (var message in request.Messages) conversation.Add((message.Role, message.Content));
            conversation.Add(("user", PromptToText(request.Question)));
        }

        var answer = await InvokeChatAsync(llm, conversation);
        conversation.Add(("assistant", answer));

        // Extract a proposed revision when the assistant made one, so the editor can diff it.
        string? suggested = null;
        var match = System.Text.RegularExpressions.Regex.Match(answer, @"<revised-prompt>\s*([\s\S]*?)\s*</revised-prompt>");
        if (match.Success) suggested = match.Groups[1].Value;

        // The tuning session is auditable: record the exchange in the run log, attributed to the
        // caller (audit columns carry the admin's username, unlike engine-written entries).
        _runLogService.AddAndSave(new Entities.AutomationRunLog(runId, entry.StepName, "explain")
        {
            ActionName = entry.ActionName,
            AnalysisName = entry.AnalysisName,
            ContentId = entry.ContentId,
            IsLLM = true,
            Prompt = PromptToText(request.Question),
            Response = answer,
            Detail = $"{{\"explainsLogId\":{entry.Id}}}",
        });

        return new JsonResult(new AutomationExplainResultModel
        {
            LogId = logId,
            Answer = answer,
            SuggestedPrompt = suggested,
            Messages = conversation.Select(m => new AutomationDebugMessageModel(m.Role, m.Content)).ToArray(),
        });
    }

    private const string ExplainSystemPrompt =
        "You are an assistant that helps an editor understand and improve one specific decision made " +
        "by an automated editorial process. You are given the exact prompt that was sent, the exact " +
        "response that came back, how the engine parsed it (the outcome), and the configuration of the " +
        "action involved.\n\n" +
        "Answer the user's question with a clear, specific explanation grounded ONLY in the provided " +
        "prompt, response, and configuration - you are reasoning about a recorded exchange, not " +
        "re-running it, so never claim certainty about what the model would do differently. When the " +
        "user asks how to improve the prompt, propose a complete revised prompt wrapped exactly in " +
        "<revised-prompt></revised-prompt> tags so it can be shown as a diff. Never claim a change was " +
        "applied - revisions are proposals the editor must review and save.";

    /// <summary>
    /// Compose the first-turn explain prompt from the log entry's recorded exchange and outcome.
    /// </summary>
    private string BuildExplainPrompt(Entities.AutomationProfile profile, Entities.AutomationRunLog entry, string question)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(PromptToText(question));
        sb.AppendLine();
        sb.AppendLine($"## The decision being examined");
        sb.AppendLine($"Profile: {profile.Name}");
        sb.AppendLine($"Step: {entry.StepName}");
        if (!string.IsNullOrWhiteSpace(entry.ActionName)) sb.AppendLine($"Action: {entry.ActionName} ({entry.ActionType})");
        if (!string.IsNullOrWhiteSpace(entry.AnalysisName)) sb.AppendLine($"Analysis: {entry.AnalysisName}");
        if (entry.ContentId.HasValue) sb.AppendLine($"Content id: {entry.ContentId}");
        sb.AppendLine($"Outcome: {entry.Outcome}");
        if (!string.IsNullOrWhiteSpace(entry.Detail)) sb.AppendLine($"Engine detail: {entry.Detail}");
        sb.AppendLine();
        if (entry.IsLLM)
        {
            sb.AppendLine("## The exact prompt that was sent");
            sb.AppendLine(entry.Prompt ?? "(not recorded)");
            sb.AppendLine();
            sb.AppendLine("## The exact response that came back");
            sb.AppendLine(entry.Response ?? "(empty)");
        }
        else
        {
            sb.AppendLine("## The engine decision (no LLM was involved)");
            sb.AppendLine(entry.Response ?? "(no description)");
        }
        if (entry.ContentId.HasValue)
        {
            var content = _contentService.FindById(entry.ContentId.Value);
            if (content != null)
            {
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
                    Status = content.Status.ToString(),
                    content.PublishedOn,
                    content.Summary,
                    Body = PromptToText(content.Body),
                }, _serializerOptions));
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Validate a v2 profile definition at save. Only malformed JSON blocks the save - a
    /// work-in-progress definition with validation errors persists as a draft (the findings
    /// panel and the run-time guard in the automation service cover invalid definitions).
    /// </summary>
    private IActionResult? ValidateDefinition(AutomationProfileModel model)
    {
        if (model.SchemaVersion < 2 || string.IsNullOrWhiteSpace(model.Definition)) return null;
        try
        {
            AutomationDefinition.Parse(model.Definition!);
            return null;
        }
        catch (System.Text.Json.JsonException ex)
        {
            return BadRequest(new { errors = new[] { new V2ValidationError("definition", $"The definition is not valid JSON: {ex.Message}") } });
        }
    }
    #endregion
}
