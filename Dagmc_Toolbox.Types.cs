using System;
using System.Drawing;


namespace Dagmc_Toolbox
{
    public enum GroupAction
    {
        Create, Append, Delete
    }

    /// <summary>
    /// see group type in DAGMC workflow doc
    ///  https://svalinn.github.io/DAGMC/usersguide/uw2.html           
    /// </summary>
    public enum GroupType
    {
        /// <summary>
        /// not valid group types
        /// </summary>
        Unknown,
        Material,
        Boundary,
        Importance
    }

    /// <summary>
    /// Boundary condition types for DAGMC workflow
    /// </summary>
    public enum BoundaryType
    {
        /// <summary>
        /// not valid bounary condition, usually indicate there is error
        /// </summary>
        Unknown,
        /// <summary>
        /// 
        /// </summary>
        White,
        /// <summary>
        /// Lambert = White: C# support multiple enum fields have the same value, kind of alias
        /// </summary>
        Lambert = White, 
        /// <summary>
        /// 
        /// </summary>
        Refelcting,
        /// <summary>
        /// 
        /// </summary>
        Isolated
    }

    partial class Helper
    {
        internal static GroupType GetGroupType(string name)
        {
            if (name.StartsWith("importance:"))
                return GroupType.Importance;
            else if (name.StartsWith("boundary:"))
                return GroupType.Boundary;
            else if (name.StartsWith("mat:"))
                return GroupType.Material;
            else
            {
                return GroupType.Unknown;
            }
        }

        internal static Color GetGroupColor(string name)
        {
            var t = GetGroupType(name);
            // todo
            return Color.White;
        }

    }
}
