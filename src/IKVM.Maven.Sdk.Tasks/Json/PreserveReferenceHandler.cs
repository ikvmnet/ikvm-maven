using System.Text.Json.Serialization;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Custom reference handler to preserve references.
    /// </summary>
    class PreserveReferenceHandler : ReferenceHandler
    {

        readonly PreserveReferenceResolver resolver;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PreserveReferenceHandler()
        {
            resolver = new PreserveReferenceResolver();
        }

        public override ReferenceResolver CreateResolver()
        {
            return resolver;
        }

    }

}
