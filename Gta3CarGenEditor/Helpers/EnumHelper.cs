using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WHampson.Gta3CarGenEditor.Helpers
{
    /// <summary>
    /// Convenient helper functions for Enums.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Gets an <see cref="Attribute"/> attached to an enum value.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="e">The enum value.</param>
        /// <returns>
        /// The <see cref="Attribute"/> instance attached to the enum value.
        /// Null of none exist.
        /// </returns>
        public static T GetAttribute<T>(Enum e) where T : Attribute
        {
            if (e == null) {
                return null;
            }

            MemberInfo[] m = e.GetType().GetMember(e.ToString());
            if (m.Count() == 0) {
                return null;
            }

            return (T) Attribute.GetCustomAttribute(m[0], typeof(T));
        }

        /// <summary>
        /// Checks whether an enum value has a specific <see cref="Attribute"/>
        /// attached to it.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="e">The enum value.</param>
        /// <returns>
        /// True if the specified enum value has the attribute attached to it,
        /// False otherwise.
        /// </returns>
        public static bool HasAttribute<T>(Enum e) where T : Attribute
        {
            if (e == null) {
                return false;
            }

            MemberInfo[] m = e.GetType().GetMember(e.ToString());
            if (m.Count() == 0) {
                return false;
            }

            return Attribute.IsDefined(m[0], typeof(T));
        }
    }
}
