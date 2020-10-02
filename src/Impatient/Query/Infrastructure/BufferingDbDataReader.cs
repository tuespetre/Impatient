using System;
using System.Buffers;
using System.Collections;
using System.Data.Common;

namespace Impatient.Query.Infrastructure
{
    public class BufferingDbDataReader : DbDataReader
    {
        private DbDataReader innerReader;
        private readonly ArrayPool<object> bufferPool;
        private object[] topBuffer;
        private object[] activeBuffer;
        private int bufferLength;
        private int bufferPosition;
        private int fieldCount;
        private int rowCount;
        private int rowIndex;

        public BufferingDbDataReader(DbDataReader innerReader, ArrayPool<object> bufferPool)
        {
            this.innerReader = innerReader;
            this.bufferPool = bufferPool;
        }

        public void Buffer()
        {
            if (topBuffer is not null)
            {
                throw new InvalidOperationException();
            }

            object[] temp = null;
            object[] buffer = null;

            try
            {
                fieldCount = innerReader.FieldCount;
                bufferLength = 1000 * fieldCount + 1;
                buffer = bufferPool.Rent(bufferLength);
                temp = bufferPool.Rent(fieldCount);

                var working = buffer;
                var pos = 0;

                while (innerReader.Read())
                {
                    rowCount++;
                    innerReader.GetValues(temp);
                    temp.CopyTo(working, pos);
                    pos += fieldCount;

                    if (pos + 2 > bufferLength)
                    {
                        var old = working;
                        working = bufferPool.Rent(bufferLength);
                        old[bufferLength - 1] = working;
                        pos = 0;
                    }
                }

                // It may not have been cleared from the ArrayPool.
                working[bufferLength - 1] = null;

                topBuffer = buffer;
                activeBuffer = buffer;
                bufferPosition = -fieldCount;
            }
            catch
            {
                if (temp is not null)
                {
                    bufferPool.Return(temp);
                }

                ReturnBuffer(buffer);
            }
            finally
            {
                innerReader.Dispose();
                innerReader = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                ReturnBuffer(topBuffer);
                innerReader?.Dispose();
                innerReader = null;
                topBuffer = null;
                activeBuffer = null;
            }
        }

        private void ReturnBuffer(object[] buffer)
        {
            if (buffer is null)
            {
                return;
            }

            var inner = buffer[buffer.Length - 1];

            ReturnBuffer((object[])inner);

            bufferPool.Return(buffer);
        }

        public override bool Read()
        {
            if (innerReader is not null)
            {
                return innerReader.Read();
            }
            else if (rowIndex < rowCount)
            {
                rowIndex++;
                bufferPosition += fieldCount;

                if (bufferPosition + 2 > bufferLength)
                {
                    activeBuffer = (object[])activeBuffer[bufferLength - 1];
                    bufferPosition = 0;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override string GetString(int ordinal)
        {
            if (innerReader is not null)
            {
                return innerReader.GetString(ordinal);
            }
            else if (activeBuffer is not null)
            {
                return (string)GetValue(ordinal);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override object GetValue(int ordinal)
        {
            if (innerReader is not null)
            {
                return innerReader.GetValue(ordinal);
            }
            else if (activeBuffer is not null)
            {
                if (ordinal >= fieldCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(ordinal));
                }

                var result = activeBuffer[bufferPosition + ordinal];

                return result;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override bool IsDBNull(int ordinal)
        {
            if (innerReader is not null)
            {
                return innerReader.IsDBNull(ordinal);
            }
            else if (activeBuffer is not null)
            {
                return DBNull.Value.Equals(GetValue(ordinal));
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        #region not implemented

        public override int Depth => throw new NotImplementedException();

        public override int FieldCount => throw new NotImplementedException();

        public override bool HasRows => throw new NotImplementedException();

        public override bool IsClosed => throw new NotImplementedException();

        public override int RecordsAffected => throw new NotImplementedException();

        public override object this[string name] => throw new NotImplementedException();

        public override object this[int ordinal] => throw new NotImplementedException();

        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override string GetName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override bool NextResult()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
