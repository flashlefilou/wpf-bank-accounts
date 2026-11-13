using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using BusinessLayer;

namespace WpfVirement
{
    /// <summary>
    /// Fenêtre de recherche des virements par compte, sens et date
    /// </summary>
    public partial class RechercheVirements : Window, INotifyPropertyChanged
    {
        private const string DebitEtCredit = "Débit et crédit";
        private const string Debit = "Débit";
        private const string Credit = "Crédit";

        private readonly ServiceCompte serviceCompte = new ServiceCompte();
        private ObservableCollection<Compte>? comptes;
        private ObservableCollection<Virement>? virements;
        private Compte? compteSelectionne;
        private string sensSelectionne = DebitEtCredit;
        private DateTime? dateSelectionnee;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string>? Sens { get; set; }

        public ObservableCollection<Compte>? Comptes
        {
            get => this.comptes;
            set
            {
                this.comptes = value;
                this.OnPropertyChanged(nameof(this.Comptes));
            }
        }

        public ObservableCollection<Virement>? Virements
        {
            get => this.virements;
            set
            {
                this.virements = value;
                this.OnPropertyChanged(nameof(this.Virements));
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

        public string SensSelectionne
        {
            get => this.sensSelectionne;
            set
            {
                this.sensSelectionne = value;
                this.OnPropertyChanged(nameof(this.SensSelectionne));
            }
        }

        public DateTime? DateSelectionnee
        {
            get => this.dateSelectionnee;
            set
            {
                this.dateSelectionnee = value;
                this.OnPropertyChanged(nameof(this.DateSelectionnee));
            }
        }

        public RechercheVirements()
        {
            this.InitializeComponent();
            this.DataContext = this;

            this.Sens = new ObservableCollection<string>();
            this.Sens.Add(DebitEtCredit);
            this.Sens.Add(Debit);
            this.Sens.Add(Credit);

            try
            {
                List<Compte>? liste = this.serviceCompte.GetAllComptes();
                this.Comptes = liste != null ? new ObservableCollection<Compte>(liste) : new ObservableCollection<Compte>();
                this.Rechercher();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Rechercher()
        {
            SensVirement sens = this.SensSelectionne switch
            {
                Debit => SensVirement.Debit,
                Credit => SensVirement.Credit,
                _ => SensVirement.Tous
            };

            this.Virements = new ObservableCollection<Virement>(
                this.serviceCompte.RechercherVirements(this.CompteSelectionne, this.DateSelectionnee, sens));
        }

        /// <summary>
        /// Affiche les virements correspondant aux filtres choisis
        /// </summary>
        private void Rechercher_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Rechercher();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Vide les filtres et réaffiche tous les virements
        /// </summary>
        private void Reinitialiser_Button_Click(object sender, RoutedEventArgs e)
        {
            this.CompteSelectionne = null;
            this.SensSelectionne = DebitEtCredit;
            this.DateSelectionnee = null;

            this.Rechercher_Button_Click(sender, e);
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
