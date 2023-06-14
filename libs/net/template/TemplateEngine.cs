using System.Collections.Concurrent;
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
            "TNO.Core",
            "TNO.Models",
            "TNO.Entities"
        };
    private readonly string[] _assemblyNames;
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
    public TemplateEngine(IRazorEngine razorEngine)
    {
        this.RazorEngine = razorEngine;
        _assemblyNames = DEFAULT_ASSEMBLIES;
    }

    /// <summary>
    /// Creates a new instance of a TemplateEngine object, initializes with specified parameters.
    /// </summary>
    /// <param name="razorEngine"></param>
    /// <param name="assemblyNames"></param>
    public TemplateEngine(IRazorEngine razorEngine, params string[] assemblyNames)
    {
        this.RazorEngine = razorEngine;
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
        return new CompiledTemplate<T>(_cache.GetOrAdd(key, i =>
        {
            return Compile(templateText);
        }));
    }
    #endregion
}
