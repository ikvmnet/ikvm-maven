using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using IKVM.Maven.Sdk.Tasks.Extensions;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Custom reference resolver for tracking instances.
    /// </summary>
    class PreserveReferenceResolver : ReferenceResolver
    {

        readonly Dictionary<string, object> referenceIdToObjectMap = new();
        readonly Dictionary<object, string> objectToReferenceIdMap = new();
        int next = 1;

        public override void AddReference(string referenceId, object value)
        {
            if (referenceIdToObjectMap.TryAdd(referenceId, value) == false)
                throw new JsonException("Duplicate reference.");
        }

        public override string GetReference(object value, out bool alreadyExists)
        {
            alreadyExists = false;

            if (objectToReferenceIdMap.TryGetValue(value, out var id))
                alreadyExists = true;
            else
                objectToReferenceIdMap.Add(value, id = next++.ToString());

            return id;
        }

        public override object ResolveReference(string referenceId)
        {
            if (referenceIdToObjectMap.TryGetValue(referenceId, out var value) == false)
                throw new JsonException("Missing reference.");

            return value;
        }

    }

}
