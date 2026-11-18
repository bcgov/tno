using RazorEngineCore;

namespace TNO.TemplateEngine;

public class CompiledTemplate<T> : ICompiledTemplate<T>
    where T : IRazorEngineTemplate
{
    #region Properties
    protected IRazorEngineCompiledTemplate<T> RazorTemplate { get; }
    #endregion

    #region Constructors
    public CompiledTemplate(IRazorEngineCompiledTemplate<T> razorTemplate)
    {
        this.RazorTemplate = razorTemplate;
    }
    #endregion

    #region Methods
    public async Task<string> RunAsync(Action<T> initializer)
    {
        return await this.RazorTemplate.RunAsync(initializer);
    }

    public string Run(Action<T> initializer)
    {
        return this.RazorTemplate.Run(initializer);
    }
    #endregion
}
