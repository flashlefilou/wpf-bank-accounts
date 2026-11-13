namespace BusinessLayer
{
    /// <summary>
    /// Exception levée quand un débit rendrait le solde d'un compte négatif
    /// </summary>
    public class SoldeInsuffisantException : Exception
    {
        public SoldeInsuffisantException() { }

        public SoldeInsuffisantException(string message)
        : base(message) { }

        public SoldeInsuffisantException(string message, Exception inner)
        : base(message, inner) { }
    }
}
