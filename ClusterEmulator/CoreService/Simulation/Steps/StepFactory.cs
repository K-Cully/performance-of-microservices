using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly string stepNamespace = typeof(StepFactory).Namespace;


        /// <summary>
        /// Creates a concrete step object from a setting value.
        /// </summary>
        /// <param name="settingValue">The step setting value.</param>
        /// <returns>An initialized step instance.</returns>
        /// <remarks>
        /// Expected setting value form:
        /// { type : <typename>, step : { <object> } }
        /// </remarks>
        public IStep Create(dynamic settingValue)
        {
            dynamic json = JsonConvert.DeserializeObject(settingValue);

            // TODO
            Type type = Type.GetType($"{stepNamespace}.{json.type.Value}");

            JObject stepJson = json.step;
            object stepObject = stepJson.ToObject(type);
            IStep step = stepObject as IStep;

            // TODO: update
            return step;
        }
    }
}
