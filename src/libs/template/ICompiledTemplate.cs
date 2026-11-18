using RazorEngineCore;

namespace TNO.TemplateEngine;

public interface ICompiledTemplate<T>
    where T : IRazorEngineTemplate
{
    #region Methods
    Task<string> RunAsync(Action<T> initializer);
    string Run(Action<T> initializer);
    #endregion
}
