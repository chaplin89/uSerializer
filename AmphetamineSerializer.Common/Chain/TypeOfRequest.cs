namespace AmphetamineSerializer.Chain
{
    /// <summary>
    /// Specify the type of request or response for a build.
    /// </summary>
    public enum TypeOfRequest
    {
        /// <summary>
        /// If used in a request, this tell to the module
        /// that will process the request that the result have to be
        /// an invokable method.
        /// </summary>
        Method,

        /// <summary>
        /// If used in a request, this tell to the module
        /// that will process the request that no output is
        /// needed because it can process the request only modifying
        /// the context.
        /// </summary>
        Context,

        /// <summary>
        /// If used in a request, this tell to the module
        /// that will process the request that the output have
        /// to be a callable delegate.
        /// </summary>
        Delegate,

        /// <summary>
        /// If used in a request, this tell to the module
        /// that will process the request that all of above 
        /// responses are valid.
        /// This is invalid if used in a response because 
        /// the module have to set the exact type of output
        /// the it has produced.
        /// </summary>
        Everything,
    }
}
