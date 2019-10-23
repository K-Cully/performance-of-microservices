using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ClusterEmulator.Service.Simulation.Test.Steps
{
    [TestClass]
    public class SimulationStepUnitTests
    {
        [TestMethod]
        public void Deserialization_RequiredData_CreatesValidInstance()
        {
            DummyStep step = JsonConvert.DeserializeObject<DummyStep>("{ }");

            Assert.IsNull(step.ParallelCount);
            Assert.AreEqual(GroupClause.Undefined, step.FailOnParallelFailures);
        }


        [TestMethod]
        public void Deserialization_AllData_CreatesValidInstance()
        {
            DummyStep step = JsonConvert.DeserializeObject<DummyStep>("{ parallelCount : 2, parallelError : 'Any' }");

            Assert.IsNotNull(step.ParallelCount);
            Assert.AreEqual<uint>(2, step.ParallelCount.Value);
            Assert.AreEqual(GroupClause.Any, step.FailOnParallelFailures);
        }


        [TestMethod]
        public void Deserialization_InvalidCount_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<DummyStep>("{ parallelCount : -1, parallelError : 'Any' }"));
        }


        [TestMethod]
        public void Deserialization_InvalidClause_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<DummyStep>("{ parallelCount : 10, parallelError : '' }"));
        }


        [TestMethod]
        public void InitializeLogger_NullLogger_ThrowsException()
        {
            IStep step = new DummyStep();

            Assert.ThrowsException<ArgumentNullException>(
                () => step.AsTypeModel(null));
        }
    }

    public class DummyStep : SimulationStep
    {
        public override Task<ExecutionStatus> ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}