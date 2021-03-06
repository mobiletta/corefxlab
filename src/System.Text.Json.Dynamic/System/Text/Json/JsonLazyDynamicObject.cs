﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Dynamic;
using System.Text.Utf8;

namespace System.Text.Json
{
    public class JsonLazyDynamicObject : DynamicObject, IDisposable
    {
        //TODO: no spans on the heap
        JsonObject _dom => default;

        private JsonLazyDynamicObject(JsonObject dom)
        {
            //TODO: no spans on the heap
            //_dom = dom;
        }

        public static JsonLazyDynamicObject Parse(ReadOnlySpan<byte> utf8Json)
        {
            var dom = JsonObject.Parse(utf8Json);
            var result = new JsonLazyDynamicObject(dom);
            return result;
        }

        public bool TryGetUInt32(Utf8String propertyName, out uint value)
        {
            JsonObject jsonObject;
            if (!_dom.TryGetValue(propertyName, out jsonObject))
            {
                value = default;
                return false;
            }
            if (jsonObject.Type != JsonObject.JsonValueType.Number)
            {
                throw new InvalidOperationException();
            }
            value = (uint)jsonObject;
            return true;
        }

        public bool TryGetString(Utf8String propertyName, out Utf8String value)
        {
            JsonObject jsonObject;
            if (!_dom.TryGetValue(propertyName, out jsonObject)) {
                value = default;
                return false;
            }
            if (jsonObject.Type != JsonObject.JsonValueType.String) {
                throw new InvalidOperationException();
            }
            value = (Utf8String)jsonObject;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            JsonObject jsonObject;
            if (!_dom.TryGetValue(binder.Name, out jsonObject)) {
                result = default;
                return false;
            }

            switch (jsonObject.Type) {
                case JsonObject.JsonValueType.Number:
                    result = (object)(int)jsonObject;
                    break;
                case JsonObject.JsonValueType.True:
                    result = (object)true;
                    break;
                case JsonObject.JsonValueType.False:
                    result = (object)false;
                    break;
                case JsonObject.JsonValueType.Null:
                    result = null;
                    break;
                case JsonObject.JsonValueType.String:
                    result = (string)jsonObject;
                    break;
                case JsonObject.JsonValueType.Object:
                    result = new JsonLazyDynamicObject(jsonObject);
                    break;
                case JsonObject.JsonValueType.Array:
                    result = new JsonLazyDynamicObject(jsonObject);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if(indexes.Length != 1 || !(indexes[0] is int)) {
                result = null;
                return false;
            }

            var index = (int)indexes[0];

            if (_dom.Type == JsonObject.JsonValueType.Array) {
                var resultObject = _dom[index];

                switch (resultObject.Type) {
                    case JsonObject.JsonValueType.Number:
                        result = (object)(int)resultObject;
                        break;
                    case JsonObject.JsonValueType.True:
                        result = (object)true;
                        break;
                    case JsonObject.JsonValueType.False:
                        result = (object)false;
                        break;
                    case JsonObject.JsonValueType.Null:
                        result = null;
                        break;
                    case JsonObject.JsonValueType.String:
                        result = (string)resultObject;
                        break;
                    case JsonObject.JsonValueType.Object:
                        result = new JsonLazyDynamicObject(resultObject);
                        break;
                    case JsonObject.JsonValueType.Array:
                        result = new JsonLazyDynamicObject(resultObject);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return true;
            }

            result = null;
            return false;
        }

        public void Dispose()
        {
            _dom.Dispose();
        }
    }
}