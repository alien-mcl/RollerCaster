using RollerCaster;

namespace Given_instance_of.MulticastObject_class.when_setting_a_property
{
    public abstract class ScenarioTest<TObject, TProperty> : MulticastObjectTest
    {
        protected abstract string ExpectedPropertyName { get; }

        protected abstract TProperty ExpectedValue { get; }

        public override void TheTest()
        {
            MulticastObject.SetProperty<TObject, TProperty>(ExpectedPropertyName, ExpectedValue);
        }
    }
}
