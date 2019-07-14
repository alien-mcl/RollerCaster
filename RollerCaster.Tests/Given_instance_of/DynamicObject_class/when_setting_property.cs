using System;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.DynamicObject_class
{
    [TestFixture]
    public class when_setting_property
    {
        private MulticastObject Proxy { get; set; }

        private DynamicObject Dynamic { get; set; }

        [Test]
        public void Should_get_dynamic_property()
        {
            object result;
            if (Dynamic.TryGetMember(Create<GetMemberBinder>(), out result))
            {
                result.Should().Be("Test");
            }
            else
            {
                ((string)((dynamic)Dynamic).Test).Should().Be("Test");
            }
        }

        [SetUp]
        public void Setup()
        {
            Proxy = new MulticastObject();
            Dynamic = (DynamicObject)Proxy.ActLike<ITestResource>();
            Dynamic.TrySetMember(Create<SetMemberBinder>(), "Test");
        }

        private T Create<T>() where T : CallSiteBinder
        {
            var arguments = new[]
            {
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            };
            return (typeof(T).Name.StartsWith("Set", StringComparison.OrdinalIgnoreCase))
                ? (T)Binder.SetMember(CSharpBinderFlags.None, "Test", typeof(ITestResource), arguments)
                : (T)Binder.GetMember(CSharpBinderFlags.None, "Test", typeof(ITestResource), arguments);
        }
    }
}
