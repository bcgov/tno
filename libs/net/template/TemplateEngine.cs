using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using RazorEngineCore;

namespace TNO.TemplateEngine;

/// <summary>
/// TemplateEngine class, provides a way to compile and cache templates.
/// Provides an in-memory cache of templates shared between all scoped instances.
/// </summary>
public class TemplateEngine<T> : ITemplateEngine<T>
    where T : IRazorEngineTemplate
{
    #region Variables
    private readonly static string[] DEFAULT_ASSEMBLIES = new[] {
            "System",
            "System.Collections",
            "System.Private.Uri",
            "System.Web.HttpUtility",
            "TNO.Core",
            "TNO.Entities",
            "TNO.Models",
            "TNO.TemplateEngine"
        };
    private readonly string[] _assemblyNames;
    private readonly ILogger<TemplateEngine<T>> _logger;
    private readonly static ConcurrentDictionary<string, IRazorEngineCompiledTemplate<T>> _cache = new();
    #endregion

    #region Properties
    /// <summary>
    /// get - The RazorEngine used to parse and render razor syntax templates.
    /// </summary>
    public IRazorEngine RazorEngine { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TemplateEngine object, initializes with specified parameters.
    /// </summary>
    /// <param name="razorEngine"></param>
    /// <param name="assemblyNames"></param>
    public TemplateEngine(IRazorEngine razorEngine, ILogger<TemplateEngine<T>> logger)
        : this(razorEngine, logger, DEFAULT_ASSEMBLIES) { }

    /// <summary>
    /// Creates a new instance of a TemplateEngine object, initializes with specified parameters.
    /// </summary>
    /// <param name="razorEngine"></param>
    /// <param name="assemblyNames"></param>
    public TemplateEngine(IRazorEngine razorEngine, ILogger<TemplateEngine<T>> logger, params string[] assemblyNames)
    {
        this.RazorEngine = razorEngine;
        _logger = logger;
        _assemblyNames = assemblyNames;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Compile the template and add common references.
    /// </summary>
    /// <param name="templateText"></param>
    /// <returns></returns>
    private IRazorEngineCompiledTemplate<T> Compile(string templateText)
    {
        return this.RazorEngine.Compile<T>(templateText, builder =>
        {
            foreach (var name in _assemblyNames)
                builder.AddAssemblyReferenceByName(name);
        });
    }

    /// <summary>
    /// Add or update the template in memory.
    /// </summary>
    /// <param name="key">Unique key to identify this template.</param>
    /// <param name="templateText"></param>
    /// <returns></returns>
    public ICompiledTemplate<T> AddOrUpdateTemplateInMemory(string key, string templateText)
    {
        var compiledTemplate = Compile(templateText);
        bool containsKey = _cache.ContainsKey(key);
        _logger.LogTrace("TemplateEngine.AddOrUpdateTemplateInMemory: Cache contains key [{key}] = [{containsKey}]", key, containsKey);
        return new CompiledTemplate<T>(_cache.AddOrUpdate(key, compiledTemplate, (key, oldValue) => compiledTemplate));
    }

    /// <summary>
    /// Get a precompiled template or compile a new template for the specified 'templateText'.
    /// </summary>
    /// <param name="key">Unique key to identify this template.</param>
    /// <param name="templateText"></param>
    /// <returns></returns>
    public ICompiledTemplate<T> GetOrAddTemplateInMemory(string key, string templateText)
    {
        var inputBytes = Encoding.UTF8.GetBytes(templateText);
        var inputHash = Convert.ToHexString(SHA256.HashData(inputBytes));
        key = $"{key}:{inputHash}";

        bool containsKey = _cache.ContainsKey(key);
        _logger.LogTrace("TemplateEngine.GetOrAddTemplateInMemory: Cache contains key [{key}] = [{containsKey}]", key, containsKey);
        return new CompiledTemplate<T>(_cache.GetOrAdd(key, i =>
        {
            _logger.LogTrace("TemplateEngine.GetOrAddTemplateInMemory: Compiling [{key}] = [{containsKey}]", key, containsKey);
            return Compile(templateText);
        }));
    }
    #endregion
}
