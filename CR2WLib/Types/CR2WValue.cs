using CR2WLib.Types.Primitive;
using CR2WLib.Types.Special;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CR2WLib.Types
{
    public abstract class CR2WValue
    {
        protected static Dictionary<string, Type> cr2wTypeFactory = new Dictionary<string, Type>();

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
        protected class CR2WType : System.Attribute
        {
            private string typeName;
            private Type type;

            public CR2WType(string TypeName, Type Type)
            {
                this.typeName = TypeName;
                this.type = Type;
            }

            public string TypeName { get => this.typeName;  }
            public Type Type { get => this.type;  }
        }

        private CR2WFile file;
        private Type valueType;
        private string[] typeName;

        public CR2WValue()
        {
            var attrs = this.GetType().GetCustomAttributes(typeof(CR2WType), false);
            foreach (CR2WType attr in attrs)
            {
                this.valueType = attr.Type;
            }
        }

        public void Init(CR2WFile file, string[] typename)
        {
            this.file = file;
            this.typeName = typename;
        }

        public CR2WFile File { get => this.file; }
        public Type Type { get => this.valueType;  }
        public string TypeName { get => this.typeName[0]; }
        public string[] FullType { get => this.typeName; }
        public string FullTypeName { get => string.Join(":", this.typeName); }
        public T As<T>()
        {
            object val = this.InternalRepresentation;
            if (val == null || typeof(T).IsAssignableFrom(val.GetType()))
                return (T)val;

            throw new TypeAccessException();
        }
        public abstract void Read(BinaryReader reader);
        public abstract object InternalRepresentation { get; }
        public virtual bool IsContainerType { get => false; }
        public static void RegisterTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var attributes = (CR2WType[])type.GetCustomAttributes(typeof(CR2WType), false);
                    if (attributes.Length > 0) {
                        CR2WValue.cr2wTypeFactory.Add(attributes[0].TypeName, type);
                    }
                }
            }
        }
        public static CProperty ReadValue(CR2WFile file, BinaryReader reader)
        {
            CProperty property = new CProperty();
            property.Init(file, new string[] { "CProperty" });
            property.Read(reader);

            return property;
        }
        public static CR2WValue ReadValue(CR2WFile file, string type, BinaryReader reader) {
            return CR2WValue.ReadValue(file, type.Split(':'), reader);
        }
        public static CR2WValue ReadValue(CR2WFile file, string[] type, BinaryReader reader) {
            if (!CR2WValue.cr2wTypeFactory.ContainsKey(type[0])) {
                CNotImplemented placeholder = new CNotImplemented();
                placeholder.Init(file, type);
                return placeholder;
            }
                //throw new TypeAccessException();

            Type valueType = CR2WValue.cr2wTypeFactory[type[0]];
            CR2WValue value = (CR2WValue)Activator.CreateInstance(valueType);

            value.Init(file, type);
            value.Read(reader);

            return value;
        }
    }
}
