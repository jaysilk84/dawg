using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

public class DictionaryAsArrayResolver : DefaultContractResolver
{
     protected override JsonContract CreateContract(Type objectType)
    {
        if (objectType.IsGenericType && objectType.Equals(typeof(Dictionary<char,Node>)))
        {
            return base.CreateArrayContract(objectType);
        }

        return base.CreateContract(objectType);
    }
}