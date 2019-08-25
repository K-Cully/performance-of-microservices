using CoreService.Simulation.HttpClientConfiguration;
using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
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
        private readonly IDictionary<string, ClientConfig> clients;
        private readonly IDictionary<string, IAsyncPolicy> policies;
        private readonly IDictionary<string, IProcessor> processors;
        private readonly IDictionary<string, IStep> steps;
        private readonly ILogger<Registry> log;


        /// <summary>
        /// A simple client factory which does not implement client caching.
        /// </summary>
        private readonly IHttpClientFactory simpleClientFactory;


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
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        public Registry(ConfigurationSettings configurationSettings, IStepFactory stepFactory,
            IConfigFactory<Processor> processorFactory, IPolicyFactory policyFactory,
            IConfigFactory<ClientConfig> clientFactory, ILogger<Registry> logger)
        {
            if (configurationSettings is null)
            {
                throw new ArgumentNullException(nameof(configurationSettings));
            }

            if (stepFactory is null)
            {
                throw new ArgumentNullException(nameof(stepFactory));
            }

            if (processorFactory is null)
            {
                throw new ArgumentNullException(nameof(processorFactory));
            }

            if (policyFactory is null)
            {
                throw new ArgumentNullException(nameof(policyFactory));
            }

            if (clientFactory is null)
            {
                throw new ArgumentNullException(nameof(clientFactory));
            }

            log = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeFromSettings(configurationSettings, ProcessorsSection, out processors, (s) => processorFactory.Create(s));
            InitializeFromSettings(configurationSettings, StepsSection, out steps, (s) => stepFactory.Create(s));
            InitializeFromSettings(configurationSettings, PoliciesSection, out policies, (s) => policyFactory.Create(s));
            InitializeFromSettings(configurationSettings, ClientsSection, out clients, (s) => clientFactory.Create(s));

            foreach (var policy in policies)
            {
                // TODO: handle non-request based policies once needed
                log.LogDebug("{Policy} added from {Setting}", policy.Value?.GetType()?.Name, policy.Key);
                PolicyRegistry.Add(policy.Key, policy.Value?.AsAsyncPolicy<HttpResponseMessage>());
            }

            simpleClientFactory = new SimpleHttpClientFactory(clients, logger);
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
        /// <returns>The <see cref="IAsyncPolicy"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// name is null or white space.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The policy is not registered or the registration is not valid.
        /// </exception>
        public IAsyncPolicy GetPolicy(string name)
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
            if (httpClientFactory is null)
            {
                throw new ArgumentNullException(nameof(httpClientFactory));
            }

            log.LogDebug("Configuring Http clients for registered steps");
            var requestSteps = steps.Values
                .Where(s => s is IRequestStep)
                .Select(s => s as IRequestStep);

            foreach (var requestStep in requestSteps)
            {
                log.LogDebug("Configuring {Client}, Reuse:{Reuse}", requestStep.ClientName, requestStep.ReuseHttpClient);
                if (requestStep.ReuseHttpClient)
                {
                    requestStep.Configure(httpClientFactory);
                }
                else
                {
                    var policies = GetClient(requestStep.ClientName).Policies
                        .Select(n => GetPolicy(n))
                        .ToArray();
                    IAsyncPolicy policy = null;
                    if (policies.Any())
                    {
                        log.LogDebug("Adding {PolicyCount} policies to {Client}", policies.Length, requestStep.ClientName);
                        policy = policies.Length == 1 ?
                            policies.First() : Policy.WrapAsync(policies);
                    }

                    requestStep.Configure(simpleClientFactory, policy);
                }
            }

            log.LogInformation("Configured Http clients for registered steps");
        }


        private void InitializeFromSettings<T>(ConfigurationSettings settings, string sectionName,
            out IDictionary<string, T> registry, Func<string, T> factory)
        {
            registry = new Dictionary<string, T>();

            if (settings.Sections.TryGetValue(sectionName, out ConfigurationSection section))
            {
                log.LogInformation("Adding settings from {ConfigSection}", sectionName);
                foreach (var property in section.Parameters)
                {
                    log.LogInformation("Adding {SettingName} from {ConfigSection}", property.Name, sectionName);
                    registry.Add(property.Name, factory(property.Value));
                }
            }
            else
            {
                log.LogError("{ConfigSection} was not found in the configuration file", sectionName);
                throw new InvalidOperationException($"Section '{sectionName}' was not found in the configuration file");
            }
        }


        private T GetRegisteredValue<T>(string name, IDictionary<string, T> registry, string registryName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                log.LogError("{RegistryName} is not valid", name);
                throw new ArgumentException($"{registryName} name cannot be null or whitespace", nameof(name));
            }

            if (!registry.TryGetValue(name, out T value))
            {
                log.LogError("{RegistryValue} was not found in {RegistryName}", name, registryName);
                throw new InvalidOperationException($"{registryName} '{name}' is not registered");
            }

            if (value == null)
            {
                log.LogError("{RegistryValue} has a null value in {RegistryName}", name, registryName);
                throw new InvalidOperationException($"Registration for {registryName} '{name}' is null");
            }

            return value;
        }
    }
}
