namespace TNO.Services.Notification.Models
{
    public interface ITnoRazorLightEngine
    {
        Task<string> CompileRenderStringAsync<T>(string key, string content, T model);
    }
}
