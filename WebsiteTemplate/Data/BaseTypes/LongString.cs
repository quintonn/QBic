using Newtonsoft.Json;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Data;
using NHibernate.Engine;
using System.Data.Common;

namespace WebsiteTemplate.Data.BaseTypes
{
    [JsonConverter(typeof(LongStringConverter))]
    /// <summary>
    /// This is pretty much a string (or that's the idea at least).
    /// But the only reason for this is to be able to indicate that we want a DB field to be NVarchar(max) or equivalent.
    /// </summary>
    public class LongString : IUserType
    {
        internal string Base { get; set; }

        public LongString()
            : this(String.Empty)
        {

        }

        public LongString(string value)
        {
            Base = value;
        }

        public static implicit operator string(LongString ls)
        {
            return ls?.Base;
        }

        public static implicit operator LongString(string ls)
        {
            return new LongString(ls);
        }

        public override string ToString()
        {
            return Base;
        }

        #region IUserType 
        public bool IsMutable
        {
            get
            {
                return false;
            }
        }

        public Type ReturnedType
        {
            get
            {
                return typeof(string);
            }
        }

        public SqlType[] SqlTypes
        {
            get
            {
                return new[] { SqlTypeFactory.GetString(int.MaxValue) };
            }
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public new bool Equals(object x, object y)
        {
            return string.Equals(x?.ToString(), y?.ToString());
        }

        public int GetHashCode(object x)
        {
            if (x == null)
            {
                return 0;
            }
            return x.GetHashCode();
        }

        //public object NullSafeGet(IDataReader rs, string[] names, object owner)
        //{
        //    object obj = NHibernateUtil.String.NullSafeGet(rs, names[0] );
        //    if (obj == null)
        //    {
        //        return null;
        //    }
            
        //    var resString = (string)obj;
        //    var res = (LongString)resString;
        //    return res;
        //}

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            if (value == null)
            {
                ((IDataParameter)cmd.Parameters[index]).Value = DBNull.Value;
            }
            else
            {
                ((IDataParameter)cmd.Parameters[index]).Value = value?.ToString();
            }
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            NullSafeSet(cmd, value, index);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            object obj = NHibernateUtil.String.NullSafeGet(rs, names[0], session, owner);
            if (obj == null)
            {
                return null;
            }

            var resString = (string)obj;
            var res = (LongString)resString;
            return res;
        }

        #endregion
    }

    public class LongStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();
            return new LongString(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());

            return;
            /* To return an entire json object, use something like this */

            /*
            var jo = new JObject();
            Type type = value.GetType();
            jo.Add("type", type.Name);

            foreach (var prop in type.GetProperties())
            {
                if (prop.CanRead)
                {
                    object propVal = prop.GetValue(value, null);
                    if (propVal != null)
                    {
                        jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
                    }
                }
            }
            jo.WriteTo(writer);*/
        }
    }
}