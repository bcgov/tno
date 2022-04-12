namespace TNO.Entities;

public interface ISaveChanges
{
    void OnAdded(User user);
    void OnModified(User user);
}
