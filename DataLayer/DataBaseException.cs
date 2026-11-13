namespace DataLayer
{
    /// <summary>
    /// Exception levée par la couche d'accès aux données
    /// </summary>
    public class DataBaseException : Exception
    {
        public DataBaseException() { }

        public DataBaseException(string message)
        : base(message) { }

        public DataBaseException(string message, Exception inner)
        : base(message, inner) { }
    }
}
