# Comptes bancaires WPF

Application de gestion de comptes bancaires en C# / WPF avec une base de données PostgreSQL.

## Fonctionnalités

- Dépôt et retrait sur un compte (`WpfComptesBancaires`)
- Virement entre deux comptes, enregistré dans un historique (`WpfVirement`)
- Création d'un compte bancaire depuis la fenêtre de virement
- Pas de découvert autorisé : un retrait ou un virement supérieur au solde est refusé

## Architecture

La solution est découpée en trois couches :

- `DataLayer` : accès à la base PostgreSQL avec Npgsql
- `BusinessLayer` : classes métier et règles de gestion
- `WpfComptesBancaires` et `WpfVirement` : les deux applications WPF, qui utilisent les mêmes couches métier et données

Les lectures passent par une vue (`vComptes`) et le virement par une fonction stockée (`sp_virement_append`).

## Installation

Prérequis : .NET 8, Visual Studio 2022 et un serveur PostgreSQL.

1. Créer une base de données et y exécuter `database/schema.sql` puis `database/sp_virement_append.sql`.
2. Copier `DataLayer/appsettings.example.json` en `DataLayer/appsettings.json` et y renseigner la chaîne de connexion.
3. Ouvrir `WpfComptesBancaires.sln`, choisir le projet de démarrage (`WpfComptesBancaires` ou `WpfVirement`) et lancer.
