using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CoreService.Simulation.Steps
{
    /// <summary>
    /// Creates step instances.
    /// </summary>
    public class StepFactory : IStepFactory
    {
        private readonly string stepNamespace = typeof(StepFactory).Namespace;


        /// <summary>
        /// Creates a concrete step object from a setting value.
        /// </summary>
        /// <param name="settingValue">The step setting value.</param>
        /// <returns>An initialized <see cref="IStep"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// { type : <typename>, step : { <object> } }
        /// </remarks>
        public IStep Create(dynamic settingValue)
        {
            // Deserialize JSON to dynamic object
            dynamic json = JsonConvert.DeserializeObject(settingValue);

            // Extract the step type
            Type type = Type.GetType($"{stepNamespace}.{json.type.Value}");
            if (type == null)
            {
                throw new InvalidOperationException($"{stepNamespace}.{json.type.Value} did not resolve to a Type");
            }

            // Convert the step JSON object to the identified concrete type
            JObject stepJson = json.step;
            return stepJson.ToObject(type) as IStep;
        }
    }
}
