using NUnit.Framework;
using RollerCaster.Data;

namespace Given_instance_of.SpecializedMulticastObject_class
{
    public abstract class SpecializedMulticastObjectTest
    {
        protected SpecializedMulticastObject MulticastObject { get; private set; }

        public virtual void TheTest()
        {
        }

        [SetUp]
        public void Setup()
        {
            MulticastObject = new SpecializedMulticastObject();
            ScenarioSetup();
            TheTest();
        }

        protected virtual void ScenarioSetup()
        {
        }
    }
}
