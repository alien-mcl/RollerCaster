using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RollerCaster.Reflection;

namespace RollerCaster
{
    /// <summary>Exposes extension methods for dynamically casting objects to other interfaces.</summary>
    public static class DynamicExtensions
    {
        internal const string AssemblyNameString = "RollerCaster.Proxies";
        private const string PreparePropertiesForIterationMethodName = "PreparePropertiesForIteration";
        private static readonly object Sync = new Object();
        private static readonly AssemblyName AssemblyName = new AssemblyName(AssemblyNameString);
#if NETSTANDARD2_0
        private static readonly AssemblyBuilder AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
#else
        private static readonly AssemblyBuilder AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
#endif
        private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyNameString + "dll");
        private static readonly MethodInfo EqualsMethodInfo = typeof(object).GetMethod(nameof(Equals), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo GetPropertyMethodInfo;
        private static readonly MethodInfo SetPropertyMethodInfo;
        private static readonly string[] AlreadyImplementedMethods = { nameof(Equals), nameof(GetHashCode) };

        private static readonly PropertyInfo TypeInstancesPropertyInfo = typeof(MulticastObject)
            .GetProperty(nameof(MulticastObject.TypeInstances), BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly PropertyInfo GetItemPropertyInfo = typeof(Dictionary<Type, object>)
            .GetProperty("Item", BindingFlags.Instance | BindingFlags.Public);

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Unified initialization of values based on the same collection.")]
        static DynamicExtensions()
        {
            foreach (var method in typeof(MulticastObject).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                switch (method.Name)
                {
                    case "GetProperty":
                        if (method.GetParameters().Length == 3)
                        {
                            GetPropertyMethodInfo = method;
                        }

                        break;
                    case "SetProperty":
                        if (method.GetParameters().Length == 4)
                        {
                            SetPropertyMethodInfo = method;
                        }

                        break;
                }

                if ((GetPropertyMethodInfo != null) && (SetPropertyMethodInfo != null))
                {
                    break;
                }
            }
        }

        internal static IDictionary<Type, ICollection<PropertyInfo>> TypeProperties { get; } = new ConcurrentDictionary<Type, ICollection<PropertyInfo>>();

        internal static IDictionary<Type, List<Type>> TypeHierarchy { get; } = new ConcurrentDictionary<Type, List<Type>>();

        /// <summary>Undoes the <see cref="DynamicExtensions.ActLike{T}(object)" />.</summary>
        /// <remarks>This method only retracts a type cast leaving properties set untouched.</remarks>
        /// <typeparam name="T">Casted type to be undone.</typeparam>
        /// <param name="instance">Casted instance.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This is a part of a fluent-like API, thus it is intended.")]
        public static void UndoActLike<T>(this object instance)
        {
            var multicastObject = instance.Unwrap();
            multicastObject.Types.Remove(typeof(T));
        }

        /// <summary>Checkes whether a given <paramref name="instance" /> was already casted to an interface of type <typeparamref name="T" />.</summary>
        /// <typeparam name="T">Type of the interface to check against.</typeparam>
        /// <param name="instance">Object to be checked.</param>
        /// <returns><b>true</b> if the <paramref name="instance"/> was already casted to type <typeparamref name="T" />; otherwise <b>false</b>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This is a part of a fluent-like API, thus it is intended.")]
        public static bool Is<T>(this object instance)
        {
            return instance.Unwrap().Types.Contains(typeof(T));
        }

        /// <summary>Unwraps the original <see cref="MulticastObject" /> from the proxy.</summary>
        /// <param name="instance">Instance to unwrap.</param>
        /// <returns>Original <see cref="MulticastObject" /> instance.</returns>
        public static MulticastObject Unwrap(this object instance)
        {
            MulticastObject multicastObject;
            if (!instance.TryUnwrap(out multicastObject))
            {
                throw new InvalidOperationException($"Unable to cast an instance that is not '{typeof(MulticastObject).Name}' related.");
            }

            return multicastObject;
        }

        /// <summary>Tries to unwraps the original <see cref="MulticastObject" /> from the proxy.</summary>
        /// <param name="instance">Instance to unwrap.</param>
        /// <param name="multicastObject">Resulting unwrapped objec.</param>
        /// <returns><b>true</b> if the unwrap is available; otherwise <b>false</b>.</returns>
        public static bool TryUnwrap(this object instance, out MulticastObject multicastObject)
        {
            var proxy = instance as IProxy;
            multicastObject = instance as MulticastObject;
            if ((proxy == null) && (multicastObject == null))
            {
                return false;
            }

            multicastObject = proxy?.WrappedObject ?? multicastObject;
            if (proxy?.CurrentCastedType.IsInterface == false)
            {
                proxy.GetType().GetMethod(PreparePropertiesForIterationMethodName, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(proxy, null);
            }

            return true;
        }

        /// <summary>Provides a given <paramref name="instance" /> as a dynamic object.</summary>
        /// <param name="instance">Object to be made dynamic.</param>
        /// <returns>Dynamic object.</returns>
        public static dynamic AsDynamic(this object instance)
        {
            if (instance == null)
            {
                return null;
            }

            MulticastObject multicastObject;
            if (instance.TryUnwrap(out multicastObject))
            {
                return multicastObject;
            }

            return instance;
        }

        /// <summary>Dynamically casts a given <paramref name="instance" /> to an interface of type <paramref name="type" />.</summary>
        /// <remarks>This method is intended to be used with data contract like interfaces without any methods or logic.</remarks>
        /// <param name="instance">Object to be casted.</param>
        /// <param name="type">Type of the interface to proxy.</param>
        /// <returns>Instance of type <paramref name="type" />.</returns>
        public static object ActLike(this object instance, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return instance.ActLikeInternal(type);
        }

        /// <summary>Dynamically casts a given <paramref name="instance" /> to an interface of type <typeparamref name="T" />.</summary>
        /// <remarks>This method is intended to be used with data contract like interfaces without any methods or logic.</remarks>
        /// <typeparam name="T">Type of the interface to proxy.</typeparam>
        /// <param name="instance">Object to be casted.</param>
        /// <returns>Instance of type <typeparamref name="T" />.</returns>
        public static T ActLike<T>(this object instance)
        {
            return (T)instance.ActLikeInternal(typeof(T));
        }

        internal static string GetName(this Type type)
        {
            if (type.IsArray)
            {
                return "ArrayOf_" + type.GetElementType().GetName();
            }

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType)
            {
                return type.Namespace.Replace(".", "_") + "_" + type.Name.Substring(0, type.Name.LastIndexOf('`')) + "Of_" +
                       String.Join("_And_", typeInfo.GenericTypeArguments.Select(genericType => genericType.GetName()));
            }

            return type.Namespace.Replace(".", "_") + "_" + type.Name;
        }

        internal static List<Type> EnsureDetailsOf(this MulticastObject instance, Type type)
        {
            List<Type> types;
            if (!TypeHierarchy.TryGetValue(type, out types))
            {
                types = new List<Type>();
                TypeHierarchy[type] = types;
                types.Add(type);
                EnsurePropertiesFor(type);
                foreach (var implementedInterface in type.GetInterfaces())
                {
                    types.Add(implementedInterface);
                    EnsurePropertiesFor(implementedInterface);
                }

                var currentBaseType = type.BaseType;
                while (currentBaseType != null)
                {
                    types.Add(currentBaseType);
                    EnsurePropertiesFor(currentBaseType);
                    currentBaseType = currentBaseType.BaseType;
                }
            }

            if (!instance.Types.Contains(type))
            {
                foreach (var actualType in types)
                {
                    instance.Types.Add(actualType);
                }
            }

            return types;
        }

        private static object ActLikeInternal(this object instance, Type type)
        {
            if (type.IsInstanceOfType(instance))
            {
                return instance;
            }

            object result;
            var multicastObject = instance.Unwrap();
            if (multicastObject.TypeInstances.TryGetValue(type, out result))
            {
                return result;
            }

            if (type.IsClass)
            {
                ValidateClassCast(multicastObject, type);
            }

            string name = $"ProxyOf_{type.GetName()}";
            var types = multicastObject.EnsureDetailsOf(type);
            Type generatedType = AssemblyBuilder.GetType(name);
            if (generatedType == null)
            {
                lock (Sync)
                {
                    generatedType = AssemblyBuilder.GetType(name) ?? CompileResultType(name, types);
                }
            }

            result = generatedType.GetConstructors().First().Invoke(new object[] { multicastObject, type });
            multicastObject.TypeInstances[type] = result;
            return result;
        }

        private static void EnsurePropertiesFor(Type type)
        {
            if (!TypeProperties.ContainsKey(type))
            {
                TypeProperties[type] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }
        }

        private static void ValidateClassCast(MulticastObject multicastObject, Type type)
        {
            var currentClass = multicastObject.Types.FirstOrDefault(_ => _.IsClass && _ != type);
            if (currentClass != null)
            {
                throw new InvalidOperationException($"Instance is already of type {currentClass} and cannot be casted to another class of type {type}.");
            }

            if (type.IsAbstract)
            {
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Except(properties.Select(property => property.GetGetMethod()))
                    .Except(properties.Select(property => property.GetSetMethod()));
                if (methods.Any(method => method.IsAbstract))
                {
                    throw new ArgumentOutOfRangeException(nameof(type), $"Abstract type {type} must implement all abstract methods.");
                }
            }
        }

        private static Type CompileResultType(string name, IList<Type> types)
        {
            var baseType = types[0].IsInterface ? typeof(DynamicObject) : types[0];
            TypeBuilder typeBuilder = GetTypeBuilder(name, types, baseType);
            FieldBuilder wrappedObjectFieldBuilder = typeBuilder.DefineField("_wrappedObject", typeof(MulticastObject), FieldAttributes.Private);
            FieldBuilder currentCastedTypeFieldBuilder = typeBuilder.DefineField("_currentCastedType", typeof(MulticastObject), FieldAttributes.Private);
            var properties = new List<PropertyInfo>();
            var propertiesWithBaseImplementation = new List<PropertyInfo>();
            var typeProperties =
                from type in types
                where type == types[0] || type.IsInterface
                from property in type.GetProperties()
                where property.CanRead && (property.GetGetMethod().IsAbstract || property.GetGetMethod().IsVirtual)
                select property;
            foreach (var typeProperty in typeProperties)
            {
                properties.Add(typeProperty);
                if (typeProperty.UseBaseImplementation())
                {
                    propertiesWithBaseImplementation.Add(typeProperty);
                }
            }

            typeBuilder.CreateConstructor(wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, baseType, propertiesWithBaseImplementation.Count > 0);
            typeBuilder.ImplementMethods(types, propertiesWithBaseImplementation);
            typeBuilder.ImplementProperties(properties, wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder);
            Type objectType = typeBuilder.CreateTypeInfo().AsType();
            return objectType;
        }

        private static void ImplementMethods(this TypeBuilder typeBuilder, IList<Type> types, ICollection<PropertyInfo> propertiesWithBaseImplementation)
        {
            typeBuilder.CreateMethodOverrideImplementation(typeof(object).GetMethod(nameof(Equals), BindingFlags.Instance | BindingFlags.Public));
            typeBuilder.CreateMethodOverrideImplementation(typeof(object).GetMethod(nameof(GetHashCode), BindingFlags.Instance | BindingFlags.Public));
            if (types[0].IsInterface)
            {
                typeBuilder.CreateMethodOverrideImplementation(typeof(DynamicObject).GetMethod(nameof(DynamicObject.TryGetMember), BindingFlags.Instance | BindingFlags.Public));
                typeBuilder.CreateMethodOverrideImplementation(typeof(DynamicObject).GetMethod(nameof(DynamicObject.TrySetMember), BindingFlags.Instance | BindingFlags.Public));
            }
            else
            {
                typeBuilder.CreatePreparePropertiesForIterationMethod(propertiesWithBaseImplementation);
            }

            var methods = from type in types
                          where type == types[0] || type.IsInterface
                          from method in type.GetMethods()
                          where !method.IsStatic && (method.IsVirtual || method.IsAbstract) && !method.IsFinal
                              && !type.GetProperties().Any(_ => _.GetAccessors().Contains(method))
                              && !AlreadyImplementedMethods.Contains(method.Name)
                          select method;
            foreach (var method in methods)
            {
                MethodInfo implementationMethod;
                if (MulticastObject.MethodImplementations.TryGetValue(method, out implementationMethod))
                {
                    typeBuilder.CreateMethodOverrideImplementation(method, implementationMethod);
                }
                else if (method.IsAbstract)
                {
                    throw new InvalidOperationException($"Unable to provide implementation for '{method.DeclaringType}.{method.Name}'.");
                }
            }
        }

        private static void ImplementProperties(
            this TypeBuilder typeBuilder,
            ICollection<PropertyInfo> properties,
            FieldBuilder wrappedObjectFieldBuilder,
            FieldBuilder currentCastedTypeFieldBuilder)
        {
            typeBuilder.CreateProperty(typeof(IProxy).GetProperty(nameof(IProxy.WrappedObject)), wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, wrappedObjectFieldBuilder);
            typeBuilder.CreateProperty(typeof(IProxy).GetProperty(nameof(IProxy.CurrentCastedType)), wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, currentCastedTypeFieldBuilder);
            foreach (var property in properties)
            {
                MethodInfo implementationMethod;
                if (!MulticastObject.PropertyImplementations.TryGetValue(property, out implementationMethod))
                {
                    implementationMethod = null;
                }

                typeBuilder.CreateProperty(property, wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, null, implementationMethod);
            }
        }

        private static void CreateConstructor(
            this TypeBuilder typeBuilder,
            FieldBuilder wrappedObjectFieldBuilder,
            FieldBuilder currentCastedTypeFieldBuilder,
            Type baseType,
            bool requiresPropertyBaseImplementation)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new[] { typeof(object), typeof(Type) });
            var constructorIl = constructorBuilder.GetILGenerator();
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Call, baseType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null));
            constructorIl.Emit(OpCodes.Nop);
            constructorIl.Emit(OpCodes.Nop);
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Ldarg_1);
            constructorIl.Emit(OpCodes.Stfld, wrappedObjectFieldBuilder);
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Ldarg_2);
            constructorIl.Emit(OpCodes.Stfld, currentCastedTypeFieldBuilder);

            if (requiresPropertyBaseImplementation)
            {
                constructorIl.Emit(OpCodes.Ldarg_1);
                constructorIl.Emit(OpCodes.Callvirt, TypeInstancesPropertyInfo.GetGetMethod(true));
                constructorIl.Emit(OpCodes.Ldarg_2);
                constructorIl.Emit(OpCodes.Ldarg_0);
                constructorIl.Emit(OpCodes.Callvirt, GetItemPropertyInfo.GetSetMethod());
            }

            constructorIl.Emit(OpCodes.Ret);
        }

        private static void CreatePreparePropertiesForIterationMethod(
            this TypeBuilder typeBuilder,
            ICollection<PropertyInfo> propertiesWithBaseImplementation)
        {
            if (propertiesWithBaseImplementation.Count > 0)
            {
                var methodBuilder = typeBuilder.DefineMethod(
                    PreparePropertiesForIterationMethodName,
                    (MethodAttributes.Assembly & (~MethodAttributes.VtableLayoutMask)) | MethodAttributes.ReuseSlot,
                    CallingConventions.HasThis);
                var methodIl = methodBuilder.GetILGenerator();
                methodIl.Emit(OpCodes.Nop);
                foreach (var property in propertiesWithBaseImplementation)
                {
                    var value = methodIl.DeclareLocal(property.PropertyType);
                    methodIl.Emit(OpCodes.Ldarg_0);
                    methodIl.Emit(OpCodes.Callvirt, property.GetGetMethod());
                    methodIl.Emit(OpCodes.Stloc, value);
                }

                methodIl.Emit(OpCodes.Ret);
            }
        }

        private static void CreateMethodOverrideImplementation(
            this TypeBuilder typeBuilder,
            MethodInfo methodToOverride,
            MethodInfo methodToCall = null)
        {
            var parameters = methodToOverride.GetParameters();
            var methodBuilder = typeBuilder.DefineMethod(
                methodToOverride.Name,
                (methodToOverride.Attributes & (~(MethodAttributes.VtableLayoutMask | MethodAttributes.Abstract))) | MethodAttributes.ReuseSlot,
                methodToOverride.CallingConvention,
                methodToOverride.ReturnType,
                methodToOverride.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
            var methodIl = methodBuilder.GetILGenerator();
            methodIl.Emit(OpCodes.Nop);
            methodIl.Emit(OpCodes.Ldarg_0);
            for (int index = 0; index < parameters.Length; index++)
            {
                methodIl.Emit((OpCode)typeof(OpCodes).GetField("Ldarg_" + (index + 1), BindingFlags.Static | BindingFlags.Public).GetValue(null));
            }

            methodIl.Emit(
                OpCodes.Call,
                methodToCall ?? typeof(MulticastObjectHelper).GetMethod(methodToOverride.Name, BindingFlags.Static | BindingFlags.NonPublic));
            if (methodToOverride.ReturnParameter != null)
            {
                methodIl.Emit(OpCodes.Ret);
            }

            typeBuilder.DefineMethodOverride(methodBuilder, methodToOverride);
        }

        private static void CreateProperty(
            this TypeBuilder typeBuilder,
            PropertyInfo property,
            FieldBuilder wrappedObjectFieldBuilder,
            FieldBuilder currentCastedTypeFieldBuilder,
            FieldBuilder specialFieldBuilder,
            MethodInfo methodToCall = null)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);
            if (methodToCall != null)
            {
                typeBuilder.CreateCustomPropertyGetter(property, propertyBuilder, methodToCall);
            }
            else
            {
                typeBuilder.CreatePropertyGetter(property, propertyBuilder, wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, specialFieldBuilder);
                if (property.CanWrite && (property.GetSetMethod().IsAbstract || property.GetSetMethod().IsVirtual))
                {
                    typeBuilder.CreatePropertySetter(property, propertyBuilder, wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder);
                }
            }
        }

        private static void CreateCustomPropertyGetter(
            this TypeBuilder typeBuilder,
            PropertyInfo property,
            PropertyBuilder propertyBuilder,
            MethodInfo implementationMethod)
        {
            MethodBuilder getterBuilder = typeBuilder.DefineMethod(
                "get_" + property.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.FamANDAssem | MethodAttributes.Family,
                property.PropertyType,
                Type.EmptyTypes);
            var getIl = getterBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Nop);
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Call, implementationMethod);
            getIl.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getterBuilder);
            typeBuilder.DefineMethodOverride(getterBuilder, property.GetMethod);
        }

        private static void CreatePropertyGetter(
            this TypeBuilder typeBuilder,
            PropertyInfo property,
            PropertyBuilder propertyBuilder,
            FieldBuilder wrappedObjectFieldBuilder,
            FieldBuilder currentCastedTypeFieldBuilder,
            FieldBuilder specialFieldBuilder)
        {
            MethodBuilder getterBuilder = typeBuilder.DefineMethod(
                "get_" + property.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.FamANDAssem | MethodAttributes.Family,
                property.PropertyType,
                Type.EmptyTypes);
            ILGenerator getIl = getterBuilder.GetILGenerator();

            if (specialFieldBuilder == null)
            {
                Label label = getIl.DefineLabel();
                getIl.Emit(OpCodes.Nop);
                getIl.Emit(OpCodes.Ldarg_0);
                getIl.Emit(OpCodes.Ldfld, wrappedObjectFieldBuilder);
                getIl.Emit(OpCodes.Ldarg_0);
                getIl.Emit(OpCodes.Ldfld, currentCastedTypeFieldBuilder);
                getIl.Emit(OpCodes.Ldtoken, property.PropertyType);
                getIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                getIl.Emit(OpCodes.Ldstr, property.Name);
                getIl.Emit(OpCodes.Callvirt, GetPropertyMethodInfo);
                getIl.Emit(property.PropertyType.GetTypeInfo().IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, property.PropertyType);
                if (property.UseBaseImplementation())
                {
                    LocalBuilder currentValue = getIl.DeclareLocal(property.PropertyType);
                    LocalBuilder resultValue = getIl.DeclareLocal(property.PropertyType);
                    LocalBuilder equalsResult = getIl.DeclareLocal(typeof(bool));
                    getIl.Emit(OpCodes.Stloc, currentValue);
                    getIl.Emit(OpCodes.Ldarg_0);
                    getIl.Emit(OpCodes.Call, property.GetGetMethod());
                    getIl.Emit(OpCodes.Stloc, resultValue);
                    getIl.Emit(OpCodes.Ldloc, resultValue);
                    if (property.PropertyType.GetTypeInfo().IsValueType)
                    {
                        getIl.Emit(OpCodes.Box, property.PropertyType);
                    }

                    getIl.Emit(OpCodes.Ldloc, currentValue);
                    if (property.PropertyType.GetTypeInfo().IsValueType)
                    {
                        getIl.Emit(OpCodes.Box, property.PropertyType);
                    }

                    getIl.Emit(OpCodes.Call, EqualsMethodInfo);
                    getIl.Emit(OpCodes.Stloc, equalsResult);
                    getIl.Emit(OpCodes.Ldloc, equalsResult);
                    getIl.Emit(OpCodes.Brtrue_S, label);
                    getIl.Emit(OpCodes.Nop);
                    getIl.Emit(OpCodes.Ldarg_0);
                    getIl.Emit(OpCodes.Ldfld, wrappedObjectFieldBuilder);
                    getIl.Emit(OpCodes.Ldarg_0);
                    getIl.Emit(OpCodes.Ldfld, currentCastedTypeFieldBuilder);
                    getIl.Emit(OpCodes.Ldtoken, property.PropertyType);
                    getIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                    getIl.Emit(OpCodes.Ldstr, property.Name);
                    getIl.Emit(OpCodes.Ldloc, resultValue);
                    if (property.PropertyType.GetTypeInfo().IsValueType)
                    {
                        getIl.Emit(OpCodes.Box, property.PropertyType);
                    }

                    getIl.Emit(OpCodes.Callvirt, SetPropertyMethodInfo);
                    getIl.Emit(OpCodes.Nop);
                    getIl.Emit(OpCodes.Ldloc, resultValue);
                    getIl.Emit(OpCodes.Stloc, currentValue);
                    getIl.Emit(OpCodes.Nop);
                    getIl.MarkLabel(label);
                    getIl.Emit(OpCodes.Ldloc, currentValue);
                }

                getIl.Emit(OpCodes.Ret);
            }
            else if (specialFieldBuilder == wrappedObjectFieldBuilder)
            {
                getIl.Emit(OpCodes.Nop);
                getIl.Emit(OpCodes.Ldarg_0);
                getIl.Emit(OpCodes.Ldfld, wrappedObjectFieldBuilder);
                getIl.Emit(OpCodes.Ret);
            }
            else
            {
                getIl.Emit(OpCodes.Nop);
                getIl.Emit(OpCodes.Ldarg_0);
                getIl.Emit(OpCodes.Ldfld, currentCastedTypeFieldBuilder);
                getIl.Emit(OpCodes.Ret);
            }

            propertyBuilder.SetGetMethod(getterBuilder);
            typeBuilder.DefineMethodOverride(getterBuilder, property.GetMethod);
        }

        private static void CreatePropertySetter(
            this TypeBuilder typeBuilder,
            PropertyInfo property,
            PropertyBuilder propertyBuilder,
            FieldBuilder wrappedObjectFieldBuilder,
            FieldBuilder currentCastedTypeFieldBuilder)
        {
            MethodBuilder setterBuilder = typeBuilder.DefineMethod(
                "set_" + property.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.FamANDAssem | MethodAttributes.Family,
                null,
                new[] { property.PropertyType });
            setterBuilder.DefineParameter(1, ParameterAttributes.In, property.Name.Substring(0, 1).ToLower() + (property.Name.Length > 1 ? property.Name.Substring(1) : String.Empty));
            var useBaseImplementation = property.UseBaseImplementation();
            ILGenerator setIl = setterBuilder.GetILGenerator();

            setIl.Emit(OpCodes.Nop);
            if (useBaseImplementation)
            {
                setIl.Emit(OpCodes.Ldarg_0);
                setIl.Emit(OpCodes.Ldarg_1);
                setIl.Emit(OpCodes.Call, property.GetSetMethod());
                setIl.Emit(OpCodes.Nop);
            }

            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldfld, wrappedObjectFieldBuilder);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldfld, currentCastedTypeFieldBuilder);
            setIl.Emit(OpCodes.Ldtoken, property.PropertyType);
            setIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
            setIl.Emit(OpCodes.Ldstr, property.Name);
            if (useBaseImplementation)
            {
                setIl.Emit(OpCodes.Ldarg_0);
                setIl.Emit(OpCodes.Call, property.GetGetMethod());
            }
            else
            {
                setIl.Emit(OpCodes.Ldarg_1);
            }

            if (property.PropertyType.GetTypeInfo().IsValueType)
            {
                setIl.Emit(OpCodes.Box, property.PropertyType);
            }

            setIl.Emit(OpCodes.Callvirt, SetPropertyMethodInfo);
            setIl.Emit(OpCodes.Nop);
            setIl.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setterBuilder);
            typeBuilder.DefineMethodOverride(setterBuilder, property.SetMethod);
        }

        private static TypeBuilder GetTypeBuilder(string name, IList<Type> types, Type baseType = null)
        {
            TypeBuilder typeBuilder = ModuleBuilder.DefineType(
                name,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                baseType == typeof(object) ? null : baseType,
                new[] { typeof(IProxy) }.Union(types.Where(type => type.IsInterface)).ToArray());
            return typeBuilder;
        }
    }
}