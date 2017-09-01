using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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
        private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyNameString + "dll");
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
            var proxy = instance as IProxy;
            var multicastObject = instance as MulticastObject;
            if ((proxy == null) && (multicastObject == null))
            {
                throw new InvalidOperationException($"Unable to cast an instance that is not '{typeof(MulticastObject).Name}' related.");
            }

            return proxy?.WrappedObject ?? multicastObject;
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

            if (!type.GetTypeInfo().IsInterface)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
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
            var mutlicastObject = instance.Unwrap();
            if (type.IsInstanceOfType(instance))
            {
                return instance;
            }

            string name = $"ProxyOf_{type.GetName()}";
            Type generatedType;
            lock (Sync)
            {
                generatedType = AssemblyBuilder.GetType(name) ?? CompileResultType(name, new[] { type }.Union(type.GetInterfaces()).ToArray());
            }

            mutlicastObject.Types.Add(type);
            var result = generatedType.GetConstructors().First().Invoke(new object[] { mutlicastObject, type });
            return result;
        }

        private static Type CompileResultType(string name, Type[] types)
        {
            TypeBuilder typeBuilder = GetTypeBuilder(name, types);
            FieldBuilder wrappedObjectFieldBuilder = typeBuilder.DefineField("_wrappedObject", typeof(MulticastObject), FieldAttributes.Private);
            FieldBuilder currentCastedTypeFieldBuilder = typeBuilder.DefineField("_currentCastedType", typeof(MulticastObject), FieldAttributes.Private);
            typeBuilder.CreateConstructor(wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder);
            typeBuilder.CreateProperty(types[0], typeof(IProxy).GetProperty("WrappedObject"), wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, wrappedObjectFieldBuilder);
            typeBuilder.CreateProperty(types[0], typeof(IProxy).GetProperty("CurrentCastedType"), wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, currentCastedTypeFieldBuilder);
            foreach (var property in from type in types from property in type.GetProperties() where property.CanRead select property)
            {
                typeBuilder.CreateProperty(types[0], property, wrappedObjectFieldBuilder, currentCastedTypeFieldBuilder, null);
            }

            Type objectType = typeBuilder.CreateTypeInfo().AsType();
            return objectType;
        }

        private static void CreateConstructor(this TypeBuilder typeBuilder, FieldBuilder wrappedObjectFieldBuilder, FieldBuilder currentCastedTypeFieldBuilder)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new[] { typeof(object), typeof(Type) });
            var constructorIl = constructorBuilder.GetILGenerator();
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Call, typeof(ProxyBase).GetConstructor(Type.EmptyTypes));
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
            if (property.CanWrite)
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
            ILGenerator setIl = setterBuilder.GetILGenerator();

            setIl.Emit(OpCodes.Nop);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldfld, wrappedObjectFieldBuilder);
            setIl.Emit(OpCodes.Ldtoken, castedType);
            setIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
            setIl.Emit(OpCodes.Ldtoken, property.PropertyType);
            setIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
            setIl.Emit(OpCodes.Ldstr, property.Name);
            setIl.Emit(OpCodes.Ldarg_1);
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

        private static TypeBuilder GetTypeBuilder(string name, Type[] types)
        {
            TypeBuilder typeBuilder = ModuleBuilder.DefineType(
                name,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                typeof(ProxyBase),
                new[] { typeof(IProxy) }.Union(types).ToArray());
            return typeBuilder;
        }
    }
}