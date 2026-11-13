using System.ComponentModel;
using System.Windows;
using BusinessLayer;

namespace WpfVirement
{
    /// <summary>
    /// Fenêtre d'ajout d'un compte bancaire
    /// </summary>
    public partial class AjoutCompte : Window, INotifyPropertyChanged
    {
        private readonly ServiceCompte serviceCompte = new ServiceCompte();
        private string numeroCompte = string.Empty;
        private string soldeCompte = "0";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string NumeroCompte
        {
            get => this.numeroCompte;
            set
            {
                this.numeroCompte = value;
                this.OnPropertyChanged(nameof(this.NumeroCompte));
            }
        }

        public string SoldeCompte
        {
            get => this.soldeCompte;
            set
            {
                this.soldeCompte = value;
                this.OnPropertyChanged(nameof(this.SoldeCompte));
            }
        }

        // Vrai si au moins un compte a été créé depuis l'ouverture de la fenêtre
        public bool CompteCree { get; private set; }

        public AjoutCompte()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// Crée le compte à partir du numéro et du solde saisis
        /// </summary>
        private void Creer_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(this.NumeroCompte, out int numero))
            {
                MessageBox.Show("Le numéro de compte saisi n'est pas un nombre entier valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(this.SoldeCompte, out double solde))
            {
                MessageBox.Show("Le solde saisi n'est pas un nombre valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                this.serviceCompte.AjouterCompte(new Compte(numero, solde));
                this.CompteCree = true;
                MessageBox.Show("Compte créé", "Compte bancaire", MessageBoxButton.OK, MessageBoxImage.Information);

                this.NumeroCompte = string.Empty;
                this.SoldeCompte = "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
