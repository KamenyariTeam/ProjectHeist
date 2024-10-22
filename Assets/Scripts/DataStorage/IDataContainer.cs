using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataStorage
{
    [Serializable]
    public class TableID: IEquatable<TableID>
    {
        public static readonly TableID NONE = new(nameof(NONE));

        [SerializeField]
        protected string _id;

        public TableID(string id)
        {
            _id = id;
        }

        public TableID()
        {
            _id = NONE._id;
        }

        public static bool operator ==(TableID left, TableID right)
        {
            if (ReferenceEquals(left, right)) return true;

            if (left is null || right is null) return false;

            return left.Equals(right);
        }

        public static bool operator !=(TableID left, TableID right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is TableID other && Equals(other);
        }

        public bool Equals(TableID other)
        {
            return other._id.Equals(_id);
        }

        public override int GetHashCode() => _id.GetHashCode();

        public override string ToString() => _id;

    }

    [Serializable]
    public class TableRowBase
    {
    }

    public interface IIdentifiable
    {
        public IEnumerable<TableID> Identifiers { get; }
    }


    // this is not defined as interface only because it needs to inherit from ScriptableObject to be used in unity scripts
    public abstract class IDataContainer<T>: ScriptableObject, IIdentifiable where T: TableRowBase
    {
        public abstract IEnumerable<KeyValuePair<TableID, T>> Rows { get; }
        public abstract IEnumerable<TableID> Identifiers { get; }
        public abstract bool Get(TableID id, out T row);

    }


}

