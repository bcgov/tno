namespace TNO.Kafka.Models;

public class Author
{
    #region Properties
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Url { get; set; } = "";
    #endregion

    #region Constructors
    public Author() { }

    public Author(string name)
    {
        this.Name = name ?? "";
    }

    public Author(string name, string email) : this(name)
    {
        this.Email = email ?? "";
    }

    public Author(string name, string email, string url) : this(name, email)
    {
        this.Url = url ?? "";
    }
    #endregion
}
