using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    /// <summary>
    /// Creates step instances.
    /// </summary>
    public class StepFactory
    {
        /// <summary>
        /// Creates a concrete step object from a setting value.
        /// </summary>
        /// <param name="settingValue">The step setting value.</param>
        /// <returns>An initialized step instance.</returns>
        /// <remarks>
        /// Expected setting value form:
        /// { type : <typename>, step : { <object> } }
        /// </remarks>
        public IStep Create(string settingValue)
        {
            // TODO
            return null;
        }
    }
}
