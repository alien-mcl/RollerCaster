using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace RollerCaster.Collections
{
    /// <summary>Provides extended supported for observable collections.</summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Using a collection name is OK here.")]
    public interface IObservableCollection : INotifyCollectionChanged
    {
        /// <summary>Gets a value indicating whether there is an event handler added to the <see cref="INotifyCollectionChanged.CollectionChanged" />.</summary>
        bool HasEventHandlerAdded { get; }

        /// <summary>Removes all event handlers added to the <see cref="INotifyCollectionChanged.CollectionChanged" />.</summary>
        void ClearCollectionChanged();
    }
}
