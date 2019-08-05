using Polly;

namespace CoreService.Simulation.HttpClient
{
    /// <summary>
    /// Creates policy instances.
    /// </summary>
    public interface IPolicyFactory
    {
        /// <summary>
        /// Creates a concrete policy object from a setting value.
        /// </summary>
        /// <param name="settingValue">The step setting value.</param>
        /// <returns>An initialized <see cref="IsPolicy"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// 
        /// TODO: fill this in
        /// 
        /// 
        /// { type : <typename>, step : { <object> } }
        /// </remarks>
        IsPolicy Create(string settingValue);
    }
}