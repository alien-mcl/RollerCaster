# RollerCaster

RollerCaster is a simple yet effective tiny library that allows you to create proxies of interfaces and DTO classes.

## Usage

Simple use cases for data structure is, well, simple. Just create new instance of the _MulticastObject_ class and 
use an extension method of _ActLike&lt;T&gt;() to wrap it with any interface or DTO class:

```csharp
var product = new MulticastObject().ActLike<IProduct>();
product.SomeValue = "some text";
var resource = product.ActLike<IResource>();
resource.Description = "some description";
foreach (var property in resource.Unwrap().Properties)
{
    Console.Write(property.Value);
}
```

## Method implementation

Since version 1.3, RollerCaster allows you to inject custom method implementations enabling you to create proxies 
not only of pure data structure interfaces and DTO classes, but also types with some logic methods.

In order to inject those implementations, you can use a fluent-like builder exposed by the _MulticastObject_ type 
named _ImplementationOf&lt;T&gt;()_, i.e.:

```csharp
MulticastObject
    .ImplementationOf<ICart>()
        .ForFunction(instance => instance.Summarize()).ImplementedBy(cart => Implementations.Summarize(cart))
        .ForAction(instance => instance.Validate()).ImplementedBy(cart => Implementations.Validate(cart));
```

There are a few requirements of methods provided as an implementation:
- method MUST be _static_ (ownin class doesn't have to be static)
- method MUST be _public_ - this is due to possible access rights issues for other visibilities
- method MUST accept first argument of the type being mapped - this works similar way as extension method does
- method MUST match parameters and returned values as the method being implemented (please bare in mind previous requirement!)

In the implementation provided you can access the instance on which the method is invoked, thus you can use only it's public members.

Still, there is a possibility of using _Unwrap()_ or _TryUnwrap()_ method to access an underlying _MulticastObject_ 
proxy and use _GetProperty_ method in conjuction with i.e. _HiddenPropertyInfo_ also available in this library to 
gain access to properties not exposed by the interfaces the proxy was casted to.

## Locking instances

Since version 1.4 it is possible to lock instance from writing to properties. This might be achieved by calling i.e.:
```csharp
var multicastObject = new MulticastObject();
if (!multicastObject.GetLockedState())
{
	multicastObject.LockInstance();
}
else
{
	multicastObject.UnlockInstance();
}
```

Locking an instance will cause property setters on proxy instances (after using _ActLike_) to throw InvalidOperationException.
Collections still can be modified as it affects only property setters. It is still possible to modify properties without proxy
through the MulticastObject API.