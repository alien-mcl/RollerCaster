using System;
using NUnit.Framework;
using RollerCaster.Collections;

namespace Given_instance_of.ObservableSet_class.when_working_with
{
    public abstract class ScenarioTest
    {
        protected ObservableSet<int> Set { get; private set; }

        protected ObservableSet<int> Primes { get; private set; }

        protected ObservableSet<int> FirstThree { get; private set; }

        [SetUp]
        public void Setup()
        {
            Set = new ObservableSet<int>(new[] { 1, 2, 3 });
            Primes = new ObservableSet<int>(new[] { 1, 2, 3, 5, 7 });
            FirstThree = new ObservableSet<int>(new[] { 0, 1, 2 });
            ScenarioSetup();
        }

        public ObservableSet<int> After(Action action)
        {
            action();
            return Set;
        }

        protected virtual void ScenarioSetup()
        {
        }
    }
}
