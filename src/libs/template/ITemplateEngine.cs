using RazorEngineCore;

namespace TNO.TemplateEngine;

public interface ITemplateEngine<T>
    where T : IRazorEngineTemplate
{
    #region Methods
    ICompiledTemplate<T> AddOrUpdateTemplateInMemory(string key, string templateText);

    ICompiledTemplate<T> GetOrAddTemplateInMemory(string key, string templateText);
    #endregion
}
