namespace AmphetamineSerializer.Model
{
    /// <summary>
    /// Represent the status about a function.
    /// </summary>
    public enum BuildedFunctionStatus
    {
        /// <summary>
        /// Emit was created and it is still editable so no MethodInfo is available.
        /// </summary>
        FunctionNotFinalized,

        /// <summary>
        /// CreateMethod was was called on the Emit so the Emit is not editable.
        /// MethodInfo is available, but all the calls to this method should be done 
        /// with the Emit before CreateType is called.
        /// </summary>
        FunctionFinalizedTypeNotFinalized
    }
}
