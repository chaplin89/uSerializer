namespace AmphetamineSerializer.Chain
{
    /// <summary>
    /// Specify how modules should handle the request.
    /// </summary>
    public enum TypeOfRequest
    {
        /// <summary>
        /// Request for building an invokable method.
        /// </summary>
        Method,

        /// <summary>
        /// Request to handle via context modification.
        /// </summary>
        Context,

        /// <summary>
        /// Request for building a delegate.
        /// </summary>
        Delegate,

        /// <summary>
        /// Let the module decide how to handle this request.
        /// In this case, the module that handle the request will
        /// specify how the request was handled.
        /// </summary>
        Everything,
    }
}
