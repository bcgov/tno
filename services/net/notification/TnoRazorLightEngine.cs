using RazorLight;
using System.Reflection;

namespace TNO.Services.Notification
{
    public class TnoRazorLightEngine : ITnoRazorLightEngine
    {
        private readonly IRazorLightEngine _engine;
        public TnoRazorLightEngine()
        {
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(Assembly.GetEntryAssembly())
                .Build();
        }
        public async Task<string> CompileRenderStringAsync<T>(string key, string content, T model)
        {
            return await _engine.CompileRenderStringAsync(key, content, model);
        }
    }
}
