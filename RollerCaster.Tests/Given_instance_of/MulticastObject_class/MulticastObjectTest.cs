using NUnit.Framework;
using RollerCaster;

namespace Given_instance_of.MulticastObject_class
{
    public abstract class MulticastObjectTest
    {
        protected MulticastObject MulticastObject { get; private set; }

        public virtual void TheTest()
        {
        }

        [SetUp]
        public void Setup()
        {
            MulticastObject = new MulticastObject();
            ScenarioSetup();
            TheTest();
        }

        protected virtual void ScenarioSetup()
        {
        }
    }
}
