﻿using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.DynamicObject_class
{
    [TestFixture]
    public class when_getting_property
    {
        private MulticastObject Proxy { get; set; }

        private dynamic Dynamic { get; set; }

        [Test]
        public void Should_set_dynamic_property()
        {
            ((string)Dynamic.Test).Should().Be("Test");
        }

        [Test]
        public void Should_set_proxy_dynamic_property()
        {
            ((string)Proxy.ActLike<IProduct>().AsDynamic().Test).Should().Be("Test");
        }

        [SetUp]
        public void Setup()
        {
            Proxy = new MulticastObject();
            Dynamic = Proxy.ActLike<ITestResource>();
            Dynamic.Test = "Test";
        }
    }
}
