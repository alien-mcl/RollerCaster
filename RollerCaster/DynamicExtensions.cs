using System;
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
        private static readonly object Sync = new Object();
        private static readonly AssemblyName AssemblyName = new AssemblyName(AssemblyNameString);
#if NETSTANDARD2_0
        private static readonly AssemblyBuilder AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
#else
        private static readonly AssemblyBuilder AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
#endif
        private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyNameString);
        private static readonly MethodInfo EqualsMethodInfo = typeof(object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo GetPropertyMethodInfo;
        private static readonly MethodInfo SetPropertyMethodInfo;

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

        /// <summary>Undoes the <see cref="DynamicExtensions.ActLike{T}" />.</summary>
        /// <remarks>This method only retracts a type cast leaving properties set untouched.</remarks>
        /// <typeparam name="T">Casted type to be undone.</typeparam>
        /// <param name="instance">Casted instance.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This is a part of a fluent-like API, thus it is intended.")]
        public static void UndoActLike<T>(this object instance)
        {
            var multicastObject = instance.Unwrap();
            multicastObject.Types.Remove(typeof(T));
            multicastObject.TypeProperties.Remove(typeof(T));
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
                foreach (var property in multicastObject.TypeProperties[proxy.CurrentCastedType])
                {
                    property.GetValue(proxy);
                }
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

        private static object ActLikeInternal(this object instance, Type type)
        {
            if (type.IsInstanceOfType(instance))
            {
                return instance;
            }

            var multicastObject = instance.Unwrap();
            if (type.IsClass)
            {
                ValidateClassCast(multicastObject, type);
            }

            string name = $"ProxyOf_{type.GetName()}";
            Type generatedType;
            if (!multicastObject.Types.Contains(type))
            {
                multicastObject.Types.Add(type);
                var types = new List<Type>();
                types.Add(type);
                multicastObject.TypeProperties[type] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var implementedInterface in type.GetInterfaces())
                {
                    types.Add(implementedInterface);
                    multicastObject.TypeProperties[implementedInterface] = implementedInterface.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                }

                var currentBaseType = type.BaseType;
                while (currentBaseType != null)
                {
                    types.Add(currentBaseType);
                    multicastObject.TypeProperties[currentBaseType] = currentBaseType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    currentBaseType = currentBaseType.BaseType;
                }

                lock (Sync)
                {
                    generatedType = AssemblyBuilder.GetType(name) ?? CompileResultType(name, types);
                }
            }
            else
            {
                generatedType = AssemblyBuilder.GetType(name);
            }

            var result = generatedType.GetConstructors().First().Invoke(new object[] { multicastObject, type });
            return result;
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
            var baseType = (types[0].IsInterface ? typeof(DynamicObject) : types[0]);
            TypeBuilder typeBuilder = GetTypeBuilder(name, types, baseType);
            FieldBuilder wrappedObjectFieldBuilder = typeBuilder.DefineField("_wrappedObject", typeof(MulticastObject), FieldAttributes.Private);
            FieldBuilder currentCastedTypeFieldBuilder = typeBuilder.DefineField("_currentCastedType", typeof(MulticastObject), FieldAttributes.Private);
            typeBuilder.CreateConstructor(wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, baseType);
            typeBuilder.CreateMethodOverrideImplementation(typeof(object).GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public));
            typeBuilder.CreateMethodOverrideImplementation(typeof(object).GetMethod("GetHashCode", BindingFlags.Instance | BindingFlags.Public));
            if (types[0].IsInterface)
            {
                typeBuilder.CreateMethodOverrideImplementation(typeof(DynamicObject).GetMethod("TryGetMember", BindingFlags.Instance | BindingFlags.Public));
                typeBuilder.CreateMethodOverrideImplementation(typeof(DynamicObject).GetMethod("TrySetMember", BindingFlags.Instance | BindingFlags.Public));
            }

            typeBuilder.CreateProperty(types[0], typeof(IProxy).GetProperty("WrappedObject"), wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, wrappedObjectFieldBuilder);
            typeBuilder.CreateProperty(types[0], typeof(IProxy).GetProperty("CurrentCastedType"), wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, currentCastedTypeFieldBuilder);
            var properties = from type in types
                             where type == types[0] || type.IsInterface
                             from property in type.GetProperties()
                             where property.CanRead
                                   && (property.GetGetMethod().IsAbstract || property.GetGetMethod().IsVirtual)
                             select property;
            foreach (var property in properties)
            {
                typeBuilder.CreateProperty(types[0], property, wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, null);
            }

            Type objectType = typeBuilder.CreateTypeInfo().AsType();
            return objectType;
        }

        private static void CreateConstructor(this TypeBuilder typeBuilder, FieldBuilder wrappedObjectFieldBuilder, FieldBuilder currentCastedTypeFieldBuilder, Type baseType)
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
            constructorIl.Emit(OpCodes.Ret);
        }

        private static void CreateMethodOverrideImplementation(this TypeBuilder typeBuilder, MethodInfo methodToOverride)
        {
            var parameters = methodToOverride.GetParameters();
            var methodBuilder = typeBuilder.DefineMethod(
                methodToOverride.Name,
                (methodToOverride.Attributes & (~MethodAttributes.VtableLayoutMask)) | MethodAttributes.ReuseSlot,
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

            methodIl.Emit(OpCodes.Call, typeof(MulticastObjectHelper).GetMethod(methodToOverride.Name, BindingFlags.Static | BindingFlags.NonPublic));
            if (methodToOverride.ReturnParameter != null)
            {
                methodIl.Emit(OpCodes.Ret);
            }

            typeBuilder.DefineMethodOverride(methodBuilder, methodToOverride);
        }

        private static void CreateProperty(
            this TypeBuilder typeBuilder,
            Type castedType,
            PropertyInfo property,
            FieldBuilder wrappedObjectFieldBuilder,
            FieldBuilder currentCastedTypeFieldBuilder,
            FieldBuilder specialFieldBuilder)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);
            typeBuilder.CreatePropertyGetter(castedType, property, propertyBuilder, wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, specialFieldBuilder);
            if (property.CanWrite && (property.GetSetMethod().IsAbstract || property.GetSetMethod().IsVirtual))
            {
                typeBuilder.CreatePropertySetter(castedType, property, propertyBuilder, wrappedObjectFieldBuilder);
            }
        }

        private static void CreatePropertyGetter(
            this TypeBuilder typeBuilder,
            Type castedType,
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
                getIl.Emit(OpCodes.Ldtoken, castedType);
                getIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
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
                    getIl.Emit(OpCodes.Ldtoken, castedType);
                    getIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
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
            Type castedType,
            PropertyInfo property,
            PropertyBuilder propertyBuilder,
            FieldBuilder wrappedObjectFieldBuilder)
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
            setIl.Emit(OpCodes.Ldtoken, castedType);
            setIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
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