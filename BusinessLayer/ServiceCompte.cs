using System.Data;
using System.Globalization;
using DataLayer;

namespace BusinessLayer
{
    /// <summary>
    /// Opérations sur les comptes bancaires
    /// </summary>
    public class ServiceCompte
    {
        private readonly DataAccess dataAccess = new DataAccess();

        /// <summary>
        /// Récupère la liste de tous les comptes de la base de données
        /// </summary>
        /// <returns>Liste des comptes bancaires</returns>
        public List<Compte>? GetAllComptes()
        {
            DataTable? dataTable = this.dataAccess.GetData("SELECT idcompte, solde FROM vComptes ORDER BY idcompte");
            List<Compte> comptes = new List<Compte>();

            if (dataTable != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    comptes.Add(new Compte(Convert.ToInt32(row["idcompte"]), Convert.ToDouble(row["solde"])));
                }
            }

            return comptes;
        }

        /// <summary>
        /// Met à jour le solde d'un compte : débit si le montant est négatif, crédit sinon
        /// </summary>
        /// <param name="compte">Compte à débiter ou créditer</param>
        /// <param name="montant">Montant de l'opération</param>
        /// <exception cref="SoldeInsuffisantException">Débit supérieur au solde du compte</exception>
        public void SetDebitCredit(Compte compte, double montant)
        {
            if (montant < 0)
            {
                this.VerifierSolde(compte, -montant);
            }

            string query = string.Format(CultureInfo.InvariantCulture,
                "UPDATE compte SET solde = solde + {0} WHERE idcompte = {1}", montant, compte.IdCompte);
            int nbLignes = this.dataAccess.SetData(query);
            if (nbLignes != 1)
            {
                throw new DataBaseException($"Erreur de base de données. Le compte {compte.IdCompte} n'a pas été mis à jour.");
            }

            compte.Solde += montant;
        }

        /// <summary>
        /// Réalise un virement entre deux comptes
        /// </summary>
        /// <param name="compteDebit">Compte à débiter</param>
        /// <param name="compteCredit">Compte à créditer</param>
        /// <param name="montant">Montant du virement</param>
        /// <exception cref="SoldeInsuffisantException">Montant supérieur au solde du compte à débiter</exception>
        public void Virement(Compte compteDebit, Compte compteCredit, double montant)
        {
            if (montant <= 0)
            {
                throw new ArgumentException("Le montant du virement doit être strictement positif.");
            }
            if (compteDebit.IdCompte == compteCredit.IdCompte)
            {
                throw new ArgumentException("Le compte à débiter et le compte à créditer doivent être différents.");
            }

            this.VerifierSolde(compteDebit, montant);

            this.dataAccess.VirementBancaire(compteDebit.IdCompte, compteCredit.IdCompte, montant);

            compteDebit.Solde -= montant;
            compteCredit.Solde += montant;
        }

        /// <summary>
        /// Vérifie que le solde en base du compte permet le débit (pas de découvert autorisé)
        /// </summary>
        private void VerifierSolde(Compte compte, double montantDebit)
        {
            DataTable? dataTable = this.dataAccess.GetData("SELECT solde FROM vComptes WHERE idcompte = " + compte.IdCompte);
            double soldeActuel = Convert.ToDouble(dataTable!.Rows[0]["solde"]);
            compte.Solde = soldeActuel;

            if (soldeActuel - montantDebit < 0)
            {
                throw new SoldeInsuffisantException($"Compte {compte.IdCompte} : Solde insuffisant");
            }
        }
    }
}
