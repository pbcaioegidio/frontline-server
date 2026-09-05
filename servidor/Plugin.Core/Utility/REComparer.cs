using System.Collections.Generic;

namespace Plugin.Core.Utility
{
    // Renombrar la clase para evitar conflictos de nombres
    public class REComparerUtility : EqualityComparer<object>
    {
        public override bool Equals(object x, object y) => x == y;

        public override int GetHashCode(object obj) => obj == null ? 0 : obj.GetHashCode();
    }
}