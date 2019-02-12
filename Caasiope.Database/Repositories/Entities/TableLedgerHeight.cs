using System;

namespace Caasiope.Database.Repositories.Entities
{
    public class TableLedgerHeight: IEquatable<TableLedgerHeight>
    {
        public TableLedgerHeight(string tableName, long height)
        {
            TableName = tableName;
            Height = height;
        }

        public string TableName { get; }
        public long Height { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TableLedgerHeight) obj);
        }

        public bool Equals(TableLedgerHeight other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TableName, other.TableName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TableName != null ? TableName.GetHashCode() : 0) * 397;
            }
        }
    }
}