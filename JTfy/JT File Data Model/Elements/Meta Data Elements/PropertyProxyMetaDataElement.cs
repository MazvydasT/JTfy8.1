namespace JTfy
{
    public class PropertyProxyMetaDataElement : BaseDataStructure
    {
        public List<MbString> PropertyKeys { get; protected set; }
        public List<Byte> PropertyValueTypes { get; protected set; }
        public List<object> PropertyValues { get; protected set; }

        public override int ByteCount
        {
            get
            {
                int size = 0;

                for (int i = 0, c = PropertyKeys.Count; i < c; ++i)
                {
                    size += PropertyKeys[i].ByteCount;

                    size += 1;

                    var propertyValueType = PropertyValueTypes[i];

                    switch (propertyValueType)
                    {
                        case 1:
                            {
                                size += ((MbString)PropertyValues[i]).ByteCount;

                                break;
                            }

                        case 2:
                            {
                                size += 4;

                                break;
                            }

                        case 3:
                            {
                                size += 4;

                                break;
                            }

                        case 4:
                            {
                                size += ((Date)PropertyValues[i]).ByteCount;

                                break;
                            }

                        default:
                            {
                                throw new Exception(String.Format("Property Value Type {0} is not recognised.", propertyValueType));
                            }
                    }
                }

                size += new MbString("").ByteCount;

                return size;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                for (int i = 0, c = PropertyKeys.Count; i < c; ++i)
                {
                    bytesList.AddRange(PropertyKeys[i].Bytes);

                    var propertyValueType = PropertyValueTypes[i];

                    bytesList.Add(propertyValueType);

                    switch (propertyValueType)
                    {
                        case 1:
                            {
                                bytesList.AddRange(((MbString)PropertyValues[i]).Bytes);

                                break;
                            }

                        case 2:
                            {
                                bytesList.AddRange(StreamUtils.ToBytes((Int32)PropertyValues[i]));

                                break;
                            }

                        case 3:
                            {
                                bytesList.AddRange(StreamUtils.ToBytes((Single)PropertyValues[i]));

                                break;
                            }

                        case 4:
                            {
                                bytesList.AddRange(((Date)PropertyValues[i]).Bytes);

                                break;
                            }

                        default:
                            {
                                throw new Exception(String.Format("Property Value Type {0} is not recognised.", propertyValueType));
                            }
                    }
                }

                bytesList.AddRange(new MbString("").Bytes);

                return bytesList.ToArray();
            }
        }

        public PropertyProxyMetaDataElement(List<string> propertyKeys, List<object> propertyValues)
        {
            var propertyKeysCount = propertyKeys.Count;

            PropertyKeys = new List<MbString>(propertyKeysCount);

            for (int i = 0; i < propertyKeysCount; ++i)
            {
                PropertyKeys.Add(new MbString(propertyKeys[i]));
            }



            var propertyValuesCount = propertyValues.Count;

            PropertyValueTypes = new List<byte>(propertyValuesCount);
            PropertyValues = new List<object>(propertyValuesCount);

            for (int i = 0; i < propertyValuesCount; ++i)
            {
                var propertyValue = propertyValues[i];
                var propertyValueTypeName = propertyValue.GetType().Name;

                switch (propertyValueTypeName)
                {
                    case "String":
                        {
                            PropertyValueTypes.Add(1);
                            PropertyValues.Add(new MbString((string)propertyValue));

                            break;
                        }

                    case "Int32":
                        {
                            PropertyValueTypes.Add(2);
                            PropertyValues.Add((int)propertyValue);

                            break;
                        }

                    case "Single":
                        {
                            PropertyValueTypes.Add(3);
                            PropertyValues.Add((float)propertyValue);

                            break;
                        }

                    case "DateTime":
                        {
                            PropertyValueTypes.Add(4);
                            PropertyValues.Add(new Date((DateTime)propertyValue));

                            break;
                        }

                    default:
                        {
                            throw new Exception(String.Format("Property value name {0} is not supported.", propertyValueTypeName));
                        }
                }
            }
        }

        public PropertyProxyMetaDataElement(Stream stream)
        {
            PropertyKeys = new List<MbString>();
            PropertyValueTypes = new List<byte>();
            PropertyValues = new List<object>();

            var propertyKey = new MbString(stream);

            while (propertyKey.Count > 0)
            {
                PropertyKeys.Add(propertyKey);

                var propertyValueType = StreamUtils.ReadByte(stream);

                PropertyValueTypes.Add(propertyValueType);

                switch (propertyValueType)
                {
                    case 1:
                        {
                            PropertyValues.Add(new MbString(stream)); // MbString

                            break;
                        }

                    case 2:
                        {
                            PropertyValues.Add(StreamUtils.ReadInt32(stream)); // Int32

                            break;
                        }

                    case 3:
                        {
                            PropertyValues.Add(StreamUtils.ReadFloat(stream)); // Single

                            break;
                        }

                    case 4:
                        {
                            PropertyValues.Add(new Date(stream)); // Date

                            break;
                        }

                    default:
                        {
                            throw new Exception(String.Format("Property Value Type {0} is not recognised.", propertyValueType));
                        }
                }

                propertyKey = new MbString(stream);
            }
        }
    }
}