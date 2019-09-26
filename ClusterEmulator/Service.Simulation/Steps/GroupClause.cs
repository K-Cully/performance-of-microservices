using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ClusterEmulator.Service.Simulation.Steps
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupClause
    {
        /// <summary>
        /// No group clause provided.
        /// </summary>
        Undefined = 0,


        /// <summary>
        /// Any group member triggers the clause.
        /// </summary>
        Any = 1,


        /// <summary>
        /// All group members required to trigger the clause.
        /// </summary>
        All = 2,


        /// <summary>
        /// No group members should trigger the clause.
        /// </summary>
        None = 3
    }
}
