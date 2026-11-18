using System;
using System.Collections.Generic;

namespace TNO.CSS.Models;

public class DataResponseModel<T>
    where T : class
{
    #region Properties
    public IEnumerable<T> Data { get; set; } = Array.Empty<T>();
    #endregion
}
