using CoreService.Simulation.HttpClientConfiguration;
using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;
using Polly;
using Polly.Registry;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Net.Http;

namespace CoreService.Simulation.Core
{
    /// <summary>
    /// Handles registration and retrieval of simulation configuration settings for a service.
    /// </summary>
    public class Registry : IRegistry
    {
        // TODO: add logigng throughtout


        private readonly IDictionary<string, ClientConfig> clients;


        private readonly IDictionary<string, Policy> policies;


        private readonly IDictionary<string, IProcessor> processors;


        private readonly IDictionary<string, IStep> steps;


        private readonly SimpleHttpClientFactory simpleClientFactory;


        /// <summary>
        /// Gets the list of http clients and their configs.
        /// </summary>
        public IEnumerable<KeyValuePair<string, ClientConfig>> Clients => clients;


        /// <summary>
        /// Gets the policy registry initialized from settings.
        /// </summary>
        public IPolicyRegistry<string> PolicyRegistry { get; private set; } = new PolicyRegistry();


        /// <summary>
        /// The identifier of the clients settings section.
        /// </summary>
        public const string ClientsSection = "Clients";


        /// <summary>
        /// The identifier of the policies settings section.
        /// </summary>
        public const string PoliciesSection = "Policies";


        /// <summary>
        /// The identifier of the processors settings section.
        /// </summary>
        public const string ProcessorsSection = "Processors";


        /// <summary>
        /// The identifier of the steps settings section.
        /// </summary>
        public const string StepsSection = "Steps";


        /// <summary>
        /// Initializes a new instance of <see cref="Registry"/>.
        /// </summary>
        /// <param name="configurationSettings">Service configuration settings from the service context.</param>
        /// <param name="stepFactory">A factory to create steps from settings.</param>
        /// <param name="processorFactory">A factory to create processors from settings.</param>
        /// <param name="policyFactory">A factory to create http client policies from settings.</param>
        /// <param name="clientFactory">A factory to create http client configurations from settings.</param>
        public Registry(ConfigurationSettings configurationSettings, IStepFactory stepFactory,
            IProcessorFactory processorFactory, IPolicyFactory policyFactory, IClientFactory clientFactory)
        {
            if (configurationSettings is null)
            {
                throw new ArgumentNullException(nameof(configurationSettings),
                    $"{nameof(configurationSettings)} cannot be null");
            }

            if (stepFactory is null)
            {
                throw new ArgumentNullException(nameof(stepFactory),
                    $"{nameof(stepFactory)} cannot be null");
            }

            if (processorFactory is null)
            {
                throw new ArgumentNullException(nameof(processorFactory),
                    $"{nameof(processorFactory)} cannot be null");
            }

            if (policyFactory is null)
            {
                throw new ArgumentNullException(nameof(policyFactory),
                    $"{nameof(policyFactory)} cannot be null");
            }

            if (clientFactory is null)
            {
                throw new ArgumentNullException(nameof(clientFactory),
                    $"{nameof(clientFactory)} cannot be null");
            }

            InitializeFromSettings(configurationSettings, ProcessorsSection, out processors, (s) => processorFactory.Create(s));
            InitializeFromSettings(configurationSettings, StepsSection, out steps, (s) => stepFactory.Create(s));
            InitializeFromSettings(configurationSettings, PoliciesSection, out policies, (s) => policyFactory.Create(s));
            InitializeFromSettings(configurationSettings, ClientsSection, out clients, (s) => clientFactory.Create(s));

            foreach (var policy in policies)
            {
                PolicyRegistry.Add(policy.Key, policy.Value);
            }

            simpleClientFactory = new SimpleHttpClientFactory
            {
                Clients = clients
            };
        }


        /// <summary>
        /// Retrieves the client config with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <returns>The <see cref="ClientConfig"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// name is null or white space.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The policy is not registered or the registration is not valid.
        /// </exception>
        public ClientConfig GetClient(string name)
        {
            return GetRegisteredValue(name, clients, "Client");
        }


        /// <summary>
        /// Retrieves the policy with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <returns>The <see cref="Policy"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// name is null or white space.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The policy is not registered or the registration is not valid.
        /// </exception>
        public Policy GetPolicy(string name)
        {
            return GetRegisteredValue(name, policies, "Policy");
        }


        /// <summary>
        /// Retrieves the processor with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the processor.</param>
        /// <returns>The <see cref="IProcessor"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// name is null or white space.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The processor is not registered or the registration is not valid.
        /// </exception>
        public IProcessor GetProcessor(string name)
        {
            return GetRegisteredValue(name, processors, "Processor");
        }


        /// <summary>
        /// Retrieves the step with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <returns>The <see cref="IStep"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// name is null or white space.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The step is not registered or the registration is not valid.
        /// </exception>
        public IStep GetStep(string name)
        {
            return GetRegisteredValue(name, steps, "Step");
        }


        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> instance to any requests steps which reuse clients.
        /// </summary>
        /// <param name="httpClientFactory">The http client factory</param>
        public void ConfigureHttpClients(IHttpClientFactory httpClientFactory)
        {
            // TODO: UTs

            var requestSteps = steps.Values
                .Where(s => s is RequestStep)
                .Select(s => s as RequestStep);

            foreach (var requestStep in requestSteps)
            {
                if (requestStep.ReuseHttpClient)
                {
                    requestStep.Configure(httpClientFactory);
                }
                else
                {
                    var policies = GetClient(requestStep.ClientName)
                        .Policies.Select(n => GetPolicy(n));
                    PolicyWrap wrap = Policy.WrapAsync(policies.ToArray());
                    requestStep.Configure(simpleClientFactory, wrap);
                }
            }
        }


        private void InitializeFromSettings<T>(ConfigurationSettings settings, string sectionName,
            out IDictionary<string, T> registry, Func<string, T> factory)
        {
            registry = new Dictionary<string, T>();

            if (settings.Sections.TryGetValue(sectionName, out ConfigurationSection section))
            {
                foreach (var property in section.Parameters)
                {
                    registry.Add(property.Name, factory(property.Value));
                }
            }
            else
            {
                throw new InvalidOperationException($"Section '{sectionName}' was not found in the configuration file");
            }
        }


        private T GetRegisteredValue<T>(string name, IDictionary<string, T> registry, string typeName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{typeName} name cannot be null or whitespace", nameof(name));
            }

            if (!registry.TryGetValue(name, out T value))
            {
                throw new InvalidOperationException($"{typeName} '{name}' is not registered");
            }

            if (value == null)
            {
                throw new InvalidOperationException($"Registration for {typeName} '{name}' is null");
            }

            return value;
        }
    }
}
