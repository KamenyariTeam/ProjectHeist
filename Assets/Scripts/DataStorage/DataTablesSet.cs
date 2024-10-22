using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace DataStorage
{

    public class DataTablesSet<T> : IDataContainer<T> where T: TableRowBase
    {
        [SerializeField]
        private List<DataTable<T>> _tables = new();

        public override IEnumerable<KeyValuePair<TableID, T>> Rows
        {
            get
            {
                foreach (var table in _tables)
                {
                    foreach (var row in table.Rows)
                    {
                        yield return row;
                    }
                }
            }
        }

        public override IEnumerable<TableID> Identifiers
        {
            get
            {
                foreach (var table in _tables)
                {
                    foreach (var id in table.Identifiers)
                    {
                        yield return id;
                    }
                }
            }
        }

        public override bool Get(TableID id, out T row)
        {
            foreach (var table in _tables)
            {
                if (table.Get(id, out row))
                {
                    return true;
                }
            }
            row = null;
            return false;
        }


    }

}

