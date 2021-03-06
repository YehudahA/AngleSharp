﻿namespace AngleSharp.Dom.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// NamedNodeNap is a key/value pair of nodes that can be accessed by
    /// numeric or string index.
    /// </summary>
    [DebuggerStepThrough]
    sealed class NamedNodeMap : INamedNodeMap
    {
        #region Fields
        
        readonly List<Attr> _items;
        readonly WeakReference<Element> _owner;
        readonly Dictionary<String, Action<String>> _attributeHandlers;

        #endregion

        #region ctor

        public NamedNodeMap(Element owner)
        {
            _items = new List<Attr>();
            _owner = new WeakReference<Element>(owner);
            _attributeHandlers = new Dictionary<String, Action<String>>();
        }

        #endregion

        #region Index

        public IAttr this[String name]
        {
            get { return GetNamedItem(name); }
        }

        public IAttr this[Int32 index]
        {
            get { return index >= 0 && index < _items.Count ? _items[index] : null; }
        }

        #endregion

        #region Internal Properties

        internal Element Owner
        {
            get
            {
                var owner = default(Element);
                _owner.TryGetTarget(out owner);
                return owner;
            }
        }

        #endregion

        #region Properties

        public Int32 Length
        {
            get { return _items.Count; }
        }

        #endregion

        #region Internal Methods

        internal void FastAddItem(Attr attr)
        {
            _items.Add(attr);

            if (attr.NamespaceUri == null)
                CallHandlers(attr.LocalName, attr.Value);
        }

        internal void RaiseChangedEvent(Attr attr, String newValue, String oldValue)
        {
            var owner = Owner;

            if (attr.NamespaceUri == null)
                CallHandlers(attr.LocalName, newValue);

            if (owner != null)
                owner.AttributeChanged(attr.LocalName, attr.NamespaceUri, oldValue);
        }

        internal void SetHandler(String name, Action<String> handler)
        {
            _attributeHandlers[name] = handler;
        }

        internal void AddHandler(String name, Action<String> handler)
        {
            if (handler != null)
            {
                var existing = default(Action<String>);

                if (_attributeHandlers.TryGetValue(name, out existing))
                    handler += existing;

                _attributeHandlers[name] = handler;
            }
        }

        internal Action<String> RemoveHandler(String name)
        {
            var handler = default(Action<String>);

            if (_attributeHandlers.TryGetValue(name, out handler))
                _attributeHandlers.Remove(name);

            return handler;
        }

        internal IAttr RemoveNamedItemOrDefault(String name)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (String.Equals(_items[i].Name, name, StringComparison.Ordinal))
                {
                    var attr = _items[i];
                    _items.RemoveAt(i);
                    attr.Container = null;
                    RaiseChangedEvent(attr, null, attr.Value);
                    return attr;
                }
            }

            return null;
        }

        internal IAttr RemoveNamedItemOrDefault(String namespaceUri, String localName)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (String.Equals(_items[i].LocalName, localName, StringComparison.Ordinal) &&
                    String.Equals(_items[i].NamespaceUri, namespaceUri, StringComparison.Ordinal))
                {
                    var attr = _items[i];
                    _items.RemoveAt(i);
                    attr.Container = null;
                    RaiseChangedEvent(attr, null, attr.Value);
                    return attr;
                }
            }

            return null;
        }

        #endregion

        #region Methods

        public IAttr GetNamedItem(String name)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (String.Equals(_items[i].Name, name, StringComparison.Ordinal))
                    return _items[i];
            }

            return null;
        }

        public IAttr GetNamedItem(String namespaceUri, String localName)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (String.Equals(_items[i].LocalName, localName, StringComparison.Ordinal) &&
                    String.Equals(_items[i].NamespaceUri, namespaceUri, StringComparison.Ordinal))
                    return _items[i];
            }

            return null;
        }

        public IAttr SetNamedItem(IAttr item)
        {
            var proposed = Prepare(item);

            if (proposed != null)
            {
                var name = item.Name;

                for (int i = 0; i < _items.Count; i++)
                {
                    if (String.Equals(_items[i].Name, name, StringComparison.Ordinal))
                    {
                        var attr = _items[i];
                        _items[i] = proposed;
                        RaiseChangedEvent(proposed, proposed.Value, attr.Value);
                        return attr;
                    }
                }

                _items.Add(proposed);
                RaiseChangedEvent(proposed, proposed.Value, null);
            }

            return null;
        }

        public IAttr SetNamedItemWithNamespaceUri(IAttr item)
        {
            var proposed = Prepare(item);

            if (proposed != null)
            {
                var localName = item.LocalName;
                var namespaceUri = item.NamespaceUri;

                for (int i = 0; i < _items.Count; i++)
                {
                    if (String.Equals(_items[i].LocalName, localName, StringComparison.Ordinal) &&
                        String.Equals(_items[i].NamespaceUri, namespaceUri, StringComparison.Ordinal))
                    {
                        var attr = _items[i];
                        _items[i] = proposed;
                        RaiseChangedEvent(proposed, proposed.Value, attr.Value);
                        return attr;
                    }
                }

                _items.Add(proposed);
                RaiseChangedEvent(proposed, proposed.Value, null);
            }

            return null;
        }

        public IAttr RemoveNamedItem(String name)
        {
            var result = RemoveNamedItemOrDefault(name);

            if (result == null)
                throw new DomException(DomError.NotFound);

            return result;
        }

        public IAttr RemoveNamedItem(String namespaceUri, String localName)
        {
            var result = RemoveNamedItemOrDefault(namespaceUri, localName);

            if (result == null)
                throw new DomException(DomError.NotFound);

            return result;
        }

        public IEnumerator<IAttr> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        
        #endregion

        #region Helpers

        void CallHandlers(String name, String value)
        {
            var handler = default(Action<String>);
            
            if (_attributeHandlers.TryGetValue(name, out handler))
                handler(value);
        }

        Attr Prepare(IAttr item)
        {
            var attr = item as Attr;

            if (attr != null)
            {
                if (attr.Container == this)
                    return null;
                else if (attr.Container != null)
                    throw new DomException(DomError.InUse);

                attr.Container = this;
            }

            return attr;
        }

        #endregion
    }
}
