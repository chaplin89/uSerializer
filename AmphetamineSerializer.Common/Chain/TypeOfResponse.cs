namespace AmphetamineSerializer.Common.Chain
{
    /// <summary>
    /// Specify how a given request was handled.
    /// </summary>
    public enum TypeOfResponse
    {
        /// <summary>
        /// The request was handled modifying the context.
        /// No methods are produced in output and no other action
        /// are needed by the consumer.
        /// </summary>
        Context,

        /// <summary>
        /// The request was handled building (or finding) a suitable method.
        /// Expect the response to contain a method.
        /// </summary>
        Method,

        /// <summary>
        /// The request was handled building (or finding) a suitable delegate.
        /// Expect the response to contain a delegate.
        /// </summary>
        Delegate,

        /// <summary>
        /// The request was handled generating an Emiter.
        /// The consumer can call this emiter.
        /// </summary>
        Emiter
    }
}
