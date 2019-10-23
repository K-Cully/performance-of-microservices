using ClusterEmulator.Service.Simulation.HttpClientConfiguration;
using ClusterEmulator.Service.Simulation.Processors;
using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ClusterEmulator.Service.Simulation.Core
{
    /// <summary>
    /// Handles registration and retrieval of simulation configuration settings for a service.
    /// </summary>
    public class Registry : IRegistry
    {
        private readonly IDictionary<string, ClientConfig> clients;
        private readonly IDictionary<string, IAsyncPolicy<HttpResponseMessage>> policies;
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
        /// <param name="settings">Configuration settings for registry initialization.</param>
        /// <param name="stepFactory">A factory to create steps from settings.</param>
        /// <param name="processorFactory">A factory to create processors from settings.</param>
        /// <param name="policyFactory">A factory to create http client policies from settings.</param>
        /// <param name="clientFactory">A factory to create http client configurations from settings.</param>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance to use for initializing loggers for created objects.</param>
        public Registry(IRegistrySettings settings,
            IConfigFactory<IStep> stepFactory,
            IConfigFactory<IProcessor> processorFactory,
            IConfigFactory<IAsyncPolicy<HttpResponseMessage>> policyFactory,
            IConfigFactory<ClientConfig> clientFactory, ILogger<Registry> logger,
            ILoggerFactory loggerFactory)
        {
            _ = settings ?? throw new ArgumentNullException(nameof(settings));
            _ = stepFactory ?? throw new ArgumentNullException(nameof(stepFactory));
            _ = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _ = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
            _ = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            InitializeFromSettings(settings, ProcessorsSection, out processors, (s) => processorFactory.Create(s));
            InitializeFromSettings(settings, StepsSection, out steps, (s) => stepFactory.Create(s));
            InitializeFromSettings(settings, PoliciesSection, out policies, (s) => policyFactory.Create(s));
            InitializeFromSettings(settings, ClientsSection, out clients, (s) => clientFactory.Create(s));

            foreach (var policy in policies)
            {
                log.LogDebug("{Policy} added from {Setting}", policy.Value?.GetType()?.Name, policy.Key);
                PolicyRegistry.Add(policy.Key, policy.Value);
            }

            simpleClientFactory = new SimpleHttpClientFactory(clients, loggerFactory.CreateLogger<SimpleHttpClientFactory>());
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
        public IAsyncPolicy<HttpResponseMessage> GetPolicy(string name)
        {
            return GetRegisteredValue(name, policies, "Policy");
        }


        /// <summary>
        /// Retrieves the processor with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the processor.</param>
        /// <returns>The <see cref="IRequestProcessor"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// name is null or white space.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The processor is not registered or the registration is not valid.
        /// </exception>
        public IRequestProcessor GetRequestProcessor(string name)
        {
            IProcessor processor = GetRegisteredValue(name, processors, "Processor");
            if (processor is IRequestProcessor)
            {
                return processor as IRequestProcessor;
            }

            log.LogError("{RegistryValue} is not a request processor", name);
            throw new InvalidOperationException($"'{name}' is not a request processor");
        }


        /// <summary>
        /// Retrieves all registered startup processors.
        /// </summary>
        /// <returns>An enumerable of registered <see cref="IStartupProcessor"/> instances.</returns>
        public IEnumerable<IStartupProcessor> GetStartupProcessors()
        {
            return processors.Values.Where(p => p is IStartupProcessor)
                .Select(p => p as IStartupProcessor);
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
                log.LogDebug("Configuring {Client}, Reuse:{Reuse}", requestStep.ClientName, requestStep.ReuseHttpMessageHandler);
                if (requestStep.ReuseHttpMessageHandler)
                {
                    requestStep.Configure(httpClientFactory);
                }
                else
                {
                    var policies = GetClient(requestStep.ClientName).Policies
                        .Select(n => GetPolicy(n))
                        .ToArray();
                    IAsyncPolicy<HttpResponseMessage> policy = null;
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


        private void InitializeFromSettings<T>(IRegistrySettings settings, string sectionName,
            out IDictionary<string, T> registry, Func<string, T> factory)
        {
            registry = new Dictionary<string, T>();

            if (settings.TryGetSection(sectionName, out IEnumerable<KeyValuePair<string, string>> section))
            {
                log.LogInformation("Adding settings from {ConfigSection}", sectionName);
                foreach (var setting in section)
                {
                    log.LogInformation("Adding {SettingName} from {ConfigSection}", setting.Key, sectionName);
                    registry.Add(setting.Key, factory(setting.Value));
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
