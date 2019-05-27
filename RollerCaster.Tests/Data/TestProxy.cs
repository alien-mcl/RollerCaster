using System;

namespace RollerCaster.Data
{
    public class TestProxy : SpecializedService
    {
        private readonly MulticastObject _wrappedObject;
        private readonly Type _currentCastedType;

        public TestProxy(MulticastObject wrappedObject, Type currentCastedType)
        {
            _wrappedObject = wrappedObject;
            _currentCastedType = currentCastedType;
        }

        public override int SomeValue
        {
            get
            {
                return (int)_wrappedObject.GetProperty(_currentCastedType, typeof(int), "SomeValue");
            }

            set
            {
                base.SomeValue = value;
                _wrappedObject.SetProperty(_currentCastedType, typeof(int), "SomeValue", base.SomeValue);
            }
        }
    }
}
