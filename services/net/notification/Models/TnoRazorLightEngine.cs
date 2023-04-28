using RazorLight;

namespace TNO.Services.Notification.Models
{
    public class TnoRazorLightEngine : ITnoRazorLightEngine
    {
        private readonly IRazorLightEngine _engine;
        public TnoRazorLightEngine()
        {
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(TemplateModel))
                .Build();
        }
        public async Task<string> CompileRenderStringAsync<T>(string key, string content, T model)
        {
            return await _engine.CompileRenderStringAsync(key, content, model);
        }
    }
}
