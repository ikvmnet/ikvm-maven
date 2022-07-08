namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Describes a Maven repository.
    /// </summary>
    class MavenRepository
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="url"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public MavenRepository(string id, string url)
        {
            Id = id ?? throw new System.ArgumentNullException(nameof(id));
            Url = url ?? throw new System.ArgumentNullException(nameof(url));
        }

        /// <summary>
        /// ID of the repository.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// URL of the repository.
        /// </summary>
        public string Url { get; set; }

    }

}
