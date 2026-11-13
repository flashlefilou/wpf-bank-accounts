using Npgsql;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace DataLayer
{
    /// <summary>
    /// Classe de connexion à la base de données
    /// </summary>
    public class DataAccess
    {
        private const string FichierConfiguration = "appsettings.json";

        public NpgsqlConnection? NpgSQLConnect { get; set; }

        /// <summary>
        /// Lit la chaîne de connexion dans le fichier de configuration
        /// </summary>
        private static string LireChaineConnexion()
        {
            string chemin = Path.Combine(AppContext.BaseDirectory, FichierConfiguration);
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(chemin));
            return document.RootElement.GetProperty("ConnectionString").GetString()!;
        }

        /// <summary>
        /// Ouverture de la connexion à la base de données
        /// </summary>
        /// <exception cref="DataBaseException">Exception levée si impossible de se connecter</exception>
        private void OpenConnection()
        {
            try
            {
                NpgSQLConnect = new NpgsqlConnection
                {
                    ConnectionString = LireChaineConnexion()
                };
                NpgSQLConnect.Open();
            }
            catch (Exception ex)
            {
                string message = "Erreur de base de données. Impossible d'ouvrir la connexion à la base de données.";
#if DEBUG
                MethodBase? m = MethodBase.GetCurrentMethod();
                throw new DataBaseException(message + " L'erreur provient de la classe " + this.GetType().Name + " / Méthode : " + m!.Name + ".\nDétail : " + ex.Message, ex);
#else
                throw new DataBaseException(message);
#endif
            }
        }

        /// <summary>
        /// Déconnexion de la base de données
        /// </summary>
        private void CloseConnection()
        {
            if (NpgSQLConnect != null)
                if (NpgSQLConnect.State.Equals(System.Data.ConnectionState.Open))
                {
                    NpgSQLConnect.Close();
                }
        }

        /// <summary>
        /// Accès à des données en lecture
        /// </summary>
        /// <param name="getQuery">Requête SELECT de sélection de données</param>
        /// <returns>Retourne un DataTable contenant les lignes renvoyées par le SELECT</returns>
        /// <exception cref="DataBaseException">Exception levée si impossible de récupérer les données</exception>
        public DataTable? GetData(string getQuery)
        {
            try
            {
                OpenConnection();
                NpgsqlCommand npgsqlCommand = new NpgsqlCommand(getQuery, NpgSQLConnect);
                NpgsqlDataAdapter npgsqlAdapter = new NpgsqlDataAdapter
                {
                    SelectCommand = npgsqlCommand
                };
                DataTable dataTable = new DataTable();
                npgsqlAdapter.Fill(dataTable);
                CloseConnection();
                if (dataTable.Rows.Count > 0)
                    return dataTable;
                else
                    throw new Exception("La source ne renvoie pas de données.");
            }
            catch (Exception ex)
            {
                CloseConnection();
                string message = "Erreur de base de données. Impossible de récupérer les données.";
#if DEBUG
                MethodBase? m = MethodBase.GetCurrentMethod();
                throw new DataBaseException(message + " L'erreur provient de la classe " + this.GetType().Name + " / Méthode : " + m!.Name + ".\n"
                    + ex.Message);
#else
                throw new DataBaseException(message);
#endif
            }
        }

        /// <summary>
        /// Permet d'insérer, supprimer ou modifier des données
        /// </summary>
        /// <param name="setQuery">Requête SQL INSERT, DELETE ou UPDATE</param>
        /// <returns>Retourne le nombre de lignes ajoutées/supprimées/modifiées</returns>
        /// <exception cref="DataBaseException">Exception levée si impossible de mettre à jour les données</exception>
        public int SetData(string setQuery)
        {
            try
            {
                OpenConnection();
                NpgsqlCommand sqlCommand = new NpgsqlCommand(setQuery, NpgSQLConnect);
                int modifiedLines = sqlCommand.ExecuteNonQuery();
                CloseConnection();
                return modifiedLines;
            }
            catch (Exception ex)
            {
                CloseConnection();
                string message = "Erreur de base de données. Impossible de mettre à jour les données.";
#if DEBUG
                MethodBase? m = MethodBase.GetCurrentMethod();
                throw new DataBaseException(message + " L'erreur provient de la classe " + this.GetType().Name + " / Méthode : " + m!.Name + ".\n"
                    + ex.Message);
#else
                throw new DataBaseException(message);
#endif
            }
        }

        /// <summary>
        /// Exécute la fonction stockée de virement
        /// </summary>
        /// <param name="idCompteDebit">ID du compte à débiter</param>
        /// <param name="idCompteCredit">ID du compte à créditer</param>
        /// <param name="montant">Montant du virement</param>
        /// <exception cref="DataBaseException">Exception levée si la fonction stockée est en erreur</exception>
        public void VirementBancaire(int idCompteDebit, int idCompteCredit, double montant)
        {
            try
            {
                OpenConnection();

                NpgsqlCommand npgsqlCommand = new NpgsqlCommand("SELECT sp_virement_append(@pidcomptedebit, @pidcomptecredit, @pmontant)", NpgSQLConnect);

                NpgsqlParameter paramIdCompteDebit = npgsqlCommand.Parameters.Add("pidcomptedebit", NpgsqlTypes.NpgsqlDbType.Integer);
                paramIdCompteDebit.Direction = ParameterDirection.Input;
                NpgsqlParameter paramIdCompteCredit = npgsqlCommand.Parameters.Add("pidcomptecredit", NpgsqlTypes.NpgsqlDbType.Integer);
                paramIdCompteCredit.Direction = ParameterDirection.Input;
                NpgsqlParameter paramMontant = npgsqlCommand.Parameters.Add("pmontant", NpgsqlTypes.NpgsqlDbType.Numeric);
                paramMontant.Direction = ParameterDirection.Input;

                paramIdCompteDebit.Value = idCompteDebit;
                paramIdCompteCredit.Value = idCompteCredit;
                paramMontant.Value = montant;

                npgsqlCommand.ExecuteNonQuery();
                CloseConnection();
            }
            catch (Exception ex)
            {
                CloseConnection();
                string message = "Erreur de base de données. Impossible de créer le virement.";
#if DEBUG
                MethodBase? m = MethodBase.GetCurrentMethod();
                throw new DataBaseException(message + " L'erreur provient de la classe " + this.GetType().Name + " / Méthode : " + m!.Name + ".\n"
                    + ex.Message);
#else
                throw new DataBaseException(message);
#endif
            }
        }
    }
}
