using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DataStorage
{
    [System.Serializable]
    public class DataTablesSet: IDataStorable
    {
        [SerializeField]
        private List<DataTable> dataTables = new();

        public IEnumerable<TableRowBase> Rows
        {
            get
            {
                foreach (DataTable table in dataTables)
                {
                    foreach (TableRowBase row in table.Rows)
                    {
                        yield return row;
                    }
                }
            }
        }

        public TableRowBase Get(string id)
        {
            return Rows.First(row => row.id == id);
        }

    }

}
