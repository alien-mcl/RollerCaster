using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Collections;

namespace RollerCaster
{
    internal static class AssertionsExtensions
    {
        private const string EntityTypeKeyReason = "because '{0}' is a type this entity acts as now.";
        private const string ReferenceTypeKeyReason = "because values of this property are of the reference type.";

        internal static void HavePropertySet<TEntity, TValue>(
            this GenericDictionaryAssertions<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>> properties,
            string propertyName,
            TValue value)
        {
            properties.ContainKey(typeof(TEntity), EntityTypeKeyReason, typeof(TEntity));
            var entityTypeProperties = properties.Subject[typeof(TEntity)].Should();
            entityTypeProperties.ContainKey(typeof(TValue), "because '{0}' is the type of value of this property.", typeof(TValue));
            var typeProperties = entityTypeProperties.Subject[typeof(TValue)].Should();
            typeProperties.Subject.Keys.Should().Contain(item => item.Name == propertyName, "because '{0}' is supposed to be set.", propertyName);
            typeProperties.Subject.First(item => item.Key.Name == propertyName)
                .Value.Should().Be(value, "because '{0}' is the value supposed to be set.", value);
        }

        internal static void HavePropertySet<TEntity>(
            this GenericDictionaryAssertions<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>> properties,
            string propertyName,
            object value)
        {
            properties.ContainKey(typeof(TEntity), EntityTypeKeyReason, typeof(TEntity));
            var entityTypeProperties = properties.Subject[typeof(TEntity)].Should();
            entityTypeProperties.ContainKey(typeof(void), ReferenceTypeKeyReason);
            var typeProperties = entityTypeProperties.Subject[typeof(void)].Should();
            typeProperties.Subject.Keys.Should().Contain(item => item.Name == propertyName, "because '{0}' is supposed to be set.", propertyName);
            typeProperties.Subject.First(item => item.Key.Name == propertyName)
                .Value.Should().Be(value, "because '{0}' is the value supposed to be set.", value);
        }

        internal static void HaveCollectionPropertyValues<TEntity, TValue>(
            this GenericDictionaryAssertions<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>> properties,
            string propertyName,
            params TValue[] values)
        {
            properties.ContainKey(typeof(TEntity), EntityTypeKeyReason, typeof(TEntity));
            var entityTypeProperties = properties.Subject[typeof(TEntity)].Should();
            entityTypeProperties.ContainKey(typeof(void), ReferenceTypeKeyReason);
            var typeProperties = entityTypeProperties.Subject[typeof(void)].Should();
            typeProperties.Subject.Keys.Should().Contain(item => item.Name == propertyName, "because '{0}' is supposed to be set.", propertyName);
            typeProperties.Subject.First(item => item.Key.Name == propertyName)
                .Value.Should().BeAssignableTo<IEnumerable>().Which.Should().Contain(values);
        }

        internal static void HaveCollectionPropertySet<TEntity>(
            this GenericDictionaryAssertions<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>> properties,
            string propertyName,
            IEnumerable value)
        {
            properties.ContainKey(typeof(TEntity), EntityTypeKeyReason, typeof(TEntity));
            var entityTypeProperties = properties.Subject[typeof(TEntity)].Should();
            entityTypeProperties.ContainKey(typeof(void), ReferenceTypeKeyReason);
            var typeProperties = entityTypeProperties.Subject[typeof(void)].Should();
            typeProperties.Subject.Keys.Should().Contain(item => item.Name == propertyName, "because '{0}' is supposed to be set.", propertyName);
            typeProperties.Subject.First(item => item.Key.Name == propertyName)
                .Value.Should().BeEquivalentTo(value, "because all values should be added when setting a collection.");
        }

        internal static void HaveDictionaryPropertySet<TEntity>(
            this GenericDictionaryAssertions<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>> properties,
            string propertyName,
            object value)
        {
            properties.ContainKey(typeof(TEntity), EntityTypeKeyReason, typeof(TEntity));
            var entityTypeProperties = properties.Subject[typeof(TEntity)].Should();
            entityTypeProperties.ContainKey(typeof(void), ReferenceTypeKeyReason);
            var typeProperties = entityTypeProperties.Subject[typeof(void)].Should();
            typeProperties.Subject.Keys.Should().Contain(item => item.Name == propertyName, "because '{0}' is supposed to be set.", propertyName);
            typeProperties.Subject.First(item => item.Key.Name == propertyName)
                .Value.Should().BeEquivalentTo(value, "because all values should be added when setting a dictionary.");
        }
    }
}
