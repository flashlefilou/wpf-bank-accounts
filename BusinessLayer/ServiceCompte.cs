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
        // La colonne solde est un numeric(10,2)
        private const double SoldeMaximum = 100000000;

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
        /// Ajoute un nouveau compte bancaire dans la base de données
        /// </summary>
        /// <param name="compte">Compte à créer</param>
        /// <exception cref="ArgumentException">Numéro ou solde invalide, ou compte déjà existant</exception>
        public void AjouterCompte(Compte compte)
        {
            if (compte.IdCompte <= 0)
            {
                throw new ArgumentException("Le numéro de compte doit être strictement positif.");
            }
            if (!double.IsFinite(compte.Solde) || compte.Solde < 0 || compte.Solde >= SoldeMaximum)
            {
                throw new ArgumentException("Le solde du compte doit être compris entre 0 et 99 999 999,99.");
            }

            // COUNT renvoie toujours une ligne, sinon GetData lèverait une exception
            DataTable? dataTable = this.dataAccess.GetData("SELECT COUNT(*) FROM vComptes WHERE idcompte = " + compte.IdCompte);
            if (Convert.ToInt32(dataTable!.Rows[0][0]) > 0)
            {
                throw new ArgumentException($"Le compte {compte.IdCompte} existe déjà.");
            }

            string query = string.Format(CultureInfo.InvariantCulture,
                "INSERT INTO compte (idcompte, solde) VALUES ({0}, {1})", compte.IdCompte, compte.Solde);
            int nbLignes = this.dataAccess.SetData(query);
            if (nbLignes != 1)
            {
                throw new DataBaseException($"Erreur de base de données. Le compte {compte.IdCompte} n'a pas été créé.");
            }
        }

        /// <summary>
        /// Recherche les virements, avec un filtre facultatif sur un compte et sur une date
        /// </summary>
        /// <param name="compte">Compte recherché, null pour tous les comptes</param>
        /// <param name="date">Date du virement, null pour toutes les dates</param>
        /// <param name="sens">Cherche le compte en débit, en crédit ou dans les deux</param>
        /// <returns>Liste des virements trouvés</returns>
        public List<Virement> RechercherVirements(Compte? compte, DateTime? date, SensVirement sens)
        {
            List<string> conditions = new List<string>();

            if (compte != null)
            {
                switch (sens)
                {
                    case SensVirement.Debit:
                        conditions.Add("idcomptedebit = " + compte.IdCompte);
                        break;
                    case SensVirement.Credit:
                        conditions.Add("idcomptecredit = " + compte.IdCompte);
                        break;
                    default:
                        conditions.Add($"(idcomptedebit = {compte.IdCompte} OR idcomptecredit = {compte.IdCompte})");
                        break;
                }
            }
            if (date != null)
            {
                conditions.Add("datetransaction = '" + date.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'");
            }

            string filtre = conditions.Count > 0 ? " WHERE " + string.Join(" AND ", conditions) : string.Empty;
            List<Virement> virements = new List<Virement>();

            // GetData lève une exception quand il n'y a aucune ligne, donc on compte d'abord
            DataTable? compteur = this.dataAccess.GetData("SELECT COUNT(*) FROM virement" + filtre);
            if (Convert.ToInt32(compteur!.Rows[0][0]) == 0)
            {
                return virements;
            }

            DataTable? dataTable = this.dataAccess.GetData(
                "SELECT idtransaction, datetransaction, idcomptedebit, idcomptecredit, montant FROM virement" + filtre + " ORDER BY idtransaction");
            foreach (DataRow row in dataTable!.Rows)
            {
                virements.Add(new Virement(
                    Convert.ToInt32(row["idtransaction"]),
                    Convert.ToDateTime(row["datetransaction"]),
                    Convert.ToInt32(row["idcomptedebit"]),
                    Convert.ToInt32(row["idcomptecredit"]),
                    Convert.ToDouble(row["montant"])));
            }

            return virements;
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
