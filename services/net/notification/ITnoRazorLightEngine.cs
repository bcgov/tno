namespace TNO.Services.Notification
{
    public interface ITnoRazorLightEngine
    {
        Task<string> CompileRenderStringAsync<T>(string key, string content, T model);
    }
}
