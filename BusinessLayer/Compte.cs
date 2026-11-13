namespace BusinessLayer
{
    /// <summary>
    /// Compte bancaire
    /// </summary>
    public class Compte
    {
        public int IdCompte { get; set; }

        public double Solde { get; set; }

        public Compte()
        {
        }

        public Compte(int idCompte, double solde)
        {
            this.IdCompte = idCompte;
            this.Solde = solde;
        }
    }
}
