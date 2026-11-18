using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TNO.DAL;

public abstract class SeedMigration : Migration
{

    #region Variables
    private readonly string _migrationPath;
    #endregion

    #region Properties
    /// <summary>
    /// get - The migration version number.
    /// </summary>
    public string Version
    {
        get
        {
            var type = this.GetType();
            var attr = type.GetCustomAttribute<MigrationAttribute>(true);

            return $"{attr?.Id.Substring(15) ?? type.Name}";
        }
    }

    /// <summary>
    /// get - The default migrations path.
    /// </summary>
    public string DefaultMigrationsPath
    {
        get
        {
            return _migrationPath;
        }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instances of a SeedMigration object.
    /// </summary>
    public SeedMigration()
    {
        _migrationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations");
    }
    #endregion

    #region Methods
    /// <summary>
    /// Execute any scripts in the migration \Up\PreUp\ folder.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="suppressTransaction"></param>
    protected void PreUp(MigrationBuilder migrationBuilder, bool suppressTransaction = false)
    {
        if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));

        ScriptDeploy(migrationBuilder, Path.Combine(this.DefaultMigrationsPath, this.Version, Path.Combine("Up", "PreUp")), "PreUp Scripts", suppressTransaction);
    }

    /// <summary>
    /// Execute any scripts in the migration \Up\ folder.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));

        ScriptDeploy(migrationBuilder, Path.Combine(this.DefaultMigrationsPath, this.Version, "Up"), "Up Scripts");
    }

    /// <summary>
    /// Execute any scripts in the migration \Up\ folder.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="suppressTransaction"></param>
    protected void Up(MigrationBuilder migrationBuilder, bool suppressTransaction)
    {
        if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));

        ScriptDeploy(migrationBuilder, Path.Combine(this.DefaultMigrationsPath, this.Version, "Up"), "Up Scripts", suppressTransaction);
    }

    /// <summary>
    /// Execute any scripts in the migration \Up\PostUp\ folder.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="suppressTransaction"></param>
    protected void PostUp(MigrationBuilder migrationBuilder, bool suppressTransaction = false)
    {
        if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));

        ScriptDeploy(migrationBuilder, Path.Combine(this.DefaultMigrationsPath, this.Version, Path.Combine("Up", "PostUp")), "PostUp Scripts", suppressTransaction);
    }

    /// <summary>
    /// Execute any scripts in the migration \Up\PreDown\ folder.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="suppressTransaction"></param>
    protected void PreDown(MigrationBuilder migrationBuilder, bool suppressTransaction = false)
    {
        if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));

        ScriptDeploy(migrationBuilder, Path.Combine(this.DefaultMigrationsPath, this.Version, Path.Combine("Down", "PreDown")), "PreDown Scripts", suppressTransaction);
    }

    /// <summary>
    /// Execute any scripts in the migration \Down\ folder.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));

        ScriptDeploy(migrationBuilder, Path.Combine(this.DefaultMigrationsPath, this.Version, "Down"), "Down Scripts");
    }

    /// <summary>
    /// Execute any scripts in the migration \Down\ folder.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="suppressTransaction"></param>
    protected void Down(MigrationBuilder migrationBuilder, bool suppressTransaction)
    {
        if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));

        ScriptDeploy(migrationBuilder, Path.Combine(this.DefaultMigrationsPath, this.Version, "Down"), "Down Scripts", suppressTransaction);
    }

    /// <summary>
    /// Execute any scripts in the migration \Down\PostDown\ folder.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="suppressTransaction"></param>
    protected void PostDown(MigrationBuilder migrationBuilder, bool suppressTransaction = false)
    {
        if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));

        ScriptDeploy(migrationBuilder, Path.Combine(this.DefaultMigrationsPath, this.Version, Path.Combine("Down", "PostDown")), "PostDown Scripts", suppressTransaction);
    }

    /// <summary>
    /// Execute the specified script or scripts in the specified folder.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="path"></param>
    /// <param name="message"></param>
    /// <param name="suppressTransaction"></param>
    protected static void ScriptDeploy(MigrationBuilder migrationBuilder, string path, string message, bool suppressTransaction = false)
    {
        if (migrationBuilder == null) throw new ArgumentNullException(nameof(migrationBuilder));
        if (path == null) throw new ArgumentNullException(nameof(path));

        migrationBuilder.Sql($"do $$ begin raise notice '{message}'; end; $$;", suppressTransaction);

        if (!Directory.Exists(path) && !File.Exists(path))
        {
            migrationBuilder.Sql($"do $$ begin raise notice 'Script does not exist {path}.'; end; $$;", suppressTransaction);
            return;
        }

        var attr = File.GetAttributes(path);
        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
        {
            var seed_files = System.IO.Directory.GetFiles(path, "*.sql").OrderBy(n => n);
            foreach (var file_name in seed_files)
            {
                ExecuteScript(migrationBuilder, file_name, suppressTransaction);
            }
        }
        else
        {
            ExecuteScript(migrationBuilder, path, suppressTransaction);
        }

    }

    /// <summary>
    /// Execute the specified script.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="path"></param>
    /// <param name="suppressTransaction"></param>
    private static void ExecuteScript(MigrationBuilder migrationBuilder, string path, bool suppressTransaction = false)
    {
        migrationBuilder.Sql($"do $$ begin raise notice '---------------> {path}'; end; $$;", suppressTransaction);
        var sql = File.ReadAllText(path).Trim();

        if (!String.IsNullOrEmpty(sql))
        {
            migrationBuilder.Sql(sql, suppressTransaction);
        }
    }
    #endregion
}
