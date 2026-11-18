namespace TNO.Core.Extensions
{
    /// <summary>
    /// ObjectExtensions static class, provides extension methods for objects.
    /// </summary>
    public static class ObjectExtensions
    {
        #region Methods
        /// <summary>
        /// Change the object's type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T? ChangeType<T>(this object value)
        {
            var t = typeof(T);
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return default;
                }

                t = Nullable.GetUnderlyingType(t) ?? typeof(T);
            }

            return (T)Convert.ChangeType(value, t);
        }

        /// <summary>
        /// Change the object's type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object? ChangeType(this object value, Type type)
        {
            var t = type;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t) ?? type;
            }

            return Convert.ChangeType(value, t);
        }
        #endregion
    }
}
