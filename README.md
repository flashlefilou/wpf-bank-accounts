# Comptes bancaires WPF

Application de bureau qui simule les opérations courantes sur des comptes bancaires : dépôt, retrait, virement, création de compte et consultation de l'historique des virements.

Elle est écrite en C# (.NET 8) avec WPF et s'appuie sur une base de données PostgreSQL. La solution est organisée en trois couches, ce qui permet à deux applications différentes de partager le même code métier et le même accès aux données.

## Fonctionnalités

- Dépôt et retrait sur un compte
- Virement entre deux comptes, enregistré dans un historique
- Création d'un compte bancaire
- Recherche des virements par compte (en débit, en crédit ou les deux) et par date
- Pas de découvert autorisé : un retrait ou un virement supérieur au solde est refusé
- Messages d'erreur détaillés en mode Debug, simplifiés en mode Release

## Architecture

```
WpfComptesBancaires --+
                      +--> BusinessLayer --> DataLayer --> PostgreSQL
WpfVirement ----------+
```

| Projet | Rôle |
| --- | --- |
| `WpfComptesBancaires` | Application de dépôt et de retrait |
| `WpfVirement` | Application de virement, avec la création de compte et la recherche des virements |
| `BusinessLayer` | Classes métier (`Compte`, `Virement`) et règles de gestion (`ServiceCompte`) |
| `DataLayer` | Accès à la base PostgreSQL avec Npgsql |

Les applications ne connaissent que la couche métier, qui est la seule à appeler la couche de données. Chaque couche a sa propre exception (`DataBaseException`, `SoldeInsuffisantException`) et les erreurs remontent jusqu'à l'interface, qui les affiche.

## Base de données

Le dossier `database` contient deux scripts :

- `schema.sql` crée les tables `compte` et `virement`, la vue `vComptes` et un jeu d'essai de trois comptes
- `sp_virement_append.sql` crée la fonction stockée qui débite un compte, crédite l'autre et enregistre le virement

Les comptes sont lus à travers la vue et le virement passe par la fonction stockée, appelée avec des paramètres.

## Installation

Prérequis : .NET 8, Visual Studio 2022 et un serveur PostgreSQL.

1. Créer une base de données et y exécuter `database/schema.sql` puis `database/sp_virement_append.sql`.
2. Copier `DataLayer/appsettings.example.json` en `DataLayer/appsettings.json` et y renseigner la chaîne de connexion. Ce fichier est ignoré par Git.
3. Ouvrir `WpfComptesBancaires.sln`, choisir le projet de démarrage (`WpfComptesBancaires` ou `WpfVirement`) et lancer.
