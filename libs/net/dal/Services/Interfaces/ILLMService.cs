using TNO.API.Areas.Admin.Models.LLM;

namespace TNO.DAL.Services;

public interface ILLMService : IBaseService<Entities.LLM, int>
{
    /// <summary>
    /// The setting that holds the connection advanced search AI prompts use.
    /// </summary>
    const string FilterAISettingName = "FilterAILlmId";

    IEnumerable<Entities.LLM> FindAll();
    Entities.LLM? FindByName(string name);

    /// <summary>
    /// Find everything that uses the specified connection the next time it runs.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    LLMUsageModel FindUsage(int id);

    /// <summary>
    /// Check whether everything using a connection can move to another one.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    /// <param name="hasDefaultApiKey">Whether reports and search can fall back to a configured API key.</param>
    /// <param name="hasDefaultDeployment">Whether reports and search can fall back to a configured deployment.</param>
    /// <returns></returns>
    LLMMigrationCheckModel CheckMigration(int id, int targetId, bool hasDefaultApiKey, bool hasDefaultDeployment);

    /// <summary>
    /// Move everything using a connection to another one, all together or not at all.
    /// Report sections keep their prompts, and a temperature outside the target's range moves to its nearest limit.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    /// <param name="hasDefaultApiKey">Whether reports and search can fall back to a configured API key.</param>
    /// <param name="hasDefaultDeployment">Whether reports and search can fall back to a configured deployment.</param>
    /// <returns></returns>
    /// <exception cref="TNO.Core.Exceptions.BadRequestException">The check found errors.</exception>
    LLMMigrationResultModel Migrate(int id, int targetId, bool hasDefaultApiKey, bool hasDefaultDeployment);
}
