using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using BusinessLayer;

namespace WpfComptesBancaires
{
    /// <summary>
    /// Fenêtre de dépôt / retrait sur un compte bancaire
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string Retrait = "Retrait";
        private const string Depot = "Dépôt";

        private readonly ServiceCompte serviceCompte = new ServiceCompte();
        private ObservableCollection<Compte>? comptes;
        private string? typeOperationSelectionne;
        private Compte? compteSelectionne;
        private string montant = "0";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string>? TypesOperation { get; set; }

        public ObservableCollection<Compte>? Comptes
        {
            get => this.comptes;
            set
            {
                this.comptes = value;
                this.OnPropertyChanged(nameof(this.Comptes));
            }
        }

        public string? TypeOperationSelectionne
        {
            get => this.typeOperationSelectionne;
            set
            {
                this.typeOperationSelectionne = value;
                this.OnPropertyChanged(nameof(this.TypeOperationSelectionne));
            }
        }

        public Compte? CompteSelectionne
        {
            get => this.compteSelectionne;
            set
            {
                this.compteSelectionne = value;
                this.OnPropertyChanged(nameof(this.CompteSelectionne));
            }
        }

        // En string pour pouvoir contrôler la saisie
        public string Montant
        {
            get => this.montant;
            set
            {
                this.montant = value;
                this.OnPropertyChanged(nameof(this.Montant));
            }
        }

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;

            this.TypesOperation = new ObservableCollection<string>();
            this.TypesOperation.Add(Retrait);
            this.TypesOperation.Add(Depot);

            try
            {
                List<Compte>? liste = this.serviceCompte.GetAllComptes();
                this.Comptes = liste != null ? new ObservableCollection<Compte>(liste) : new ObservableCollection<Compte>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Effectue le retrait ou le dépôt sur le compte sélectionné
        /// </summary>
        private void Valider_Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.TypeOperationSelectionne == null || this.CompteSelectionne == null)
            {
                MessageBox.Show("Vous devez sélectionner le type d'opération et/ou le compte bancaire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(this.Montant, out double valeur) || valeur <= 0)
            {
                MessageBox.Show("Le montant doit être un nombre strictement positif.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                double montantOperation = this.TypeOperationSelectionne == Retrait ? -valeur : valeur;
                this.serviceCompte.SetDebitCredit(this.CompteSelectionne, montantOperation);
                MessageBox.Show(this.TypeOperationSelectionne + " effectué", "Compte bancaire", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Remet le montant à 0 et vide les listes déroulantes
        /// </summary>
        private void Annuler_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Montant = "0";
            this.TypeOperationSelectionne = null;
            this.CompteSelectionne = null;
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
