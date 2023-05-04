
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;
namespace TNO.DAL.Services;

public interface IAlertService : IBaseService<Alert, int>
{
    Alert FindAlert();
}
