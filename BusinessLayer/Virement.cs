namespace BusinessLayer
{
    /// <summary>
    /// Virement enregistré entre deux comptes
    /// </summary>
    public class Virement
    {
        public int IdTransaction { get; set; }

        public DateTime DateTransaction { get; set; }

        public int IdCompteDebit { get; set; }

        public int IdCompteCredit { get; set; }

        public double Montant { get; set; }

        public Virement()
        {
        }

        public Virement(int idTransaction, DateTime dateTransaction, int idCompteDebit, int idCompteCredit, double montant)
        {
            this.IdTransaction = idTransaction;
            this.DateTransaction = dateTransaction;
            this.IdCompteDebit = idCompteDebit;
            this.IdCompteCredit = idCompteCredit;
            this.Montant = montant;
        }
    }
}
