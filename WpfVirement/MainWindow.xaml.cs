using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using BusinessLayer;

namespace WpfVirement
{
    /// <summary>
    /// Fenêtre de virement entre deux comptes bancaires
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly ServiceCompte serviceCompte = new ServiceCompte();
        private ObservableCollection<Compte>? comptes;
        private Compte? compteDebit;
        private Compte? compteCredit;
        private string montant = "0";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Compte>? Comptes
        {
            get => this.comptes;
            set
            {
                this.comptes = value;
                this.OnPropertyChanged(nameof(this.Comptes));
            }
        }

        public Compte? CompteDebit
        {
            get => this.compteDebit;
            set
            {
                this.compteDebit = value;
                this.OnPropertyChanged(nameof(this.CompteDebit));
            }
        }

        public Compte? CompteCredit
        {
            get => this.compteCredit;
            set
            {
                this.compteCredit = value;
                this.OnPropertyChanged(nameof(this.CompteCredit));
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
        /// Effectue le virement du compte à débiter vers le compte à créditer
        /// </summary>
        private void Valider_Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.CompteDebit == null || this.CompteCredit == null)
            {
                MessageBox.Show("Vous devez sélectionner le compte à débiter et le compte à créditer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(this.Montant, out double valeur))
            {
                MessageBox.Show("Le montant saisi n'est pas un nombre valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                this.serviceCompte.Virement(this.CompteDebit, this.CompteCredit, valeur);
                MessageBox.Show("Virement effectué", "Compte bancaire", MessageBoxButton.OK, MessageBoxImage.Information);
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
            this.CompteDebit = null;
            this.CompteCredit = null;
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
