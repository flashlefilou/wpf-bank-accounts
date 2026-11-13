DROP VIEW IF EXISTS vComptes;
DROP TABLE IF EXISTS Compte CASCADE;
DROP TABLE IF EXISTS Virement CASCADE;

CREATE TABLE Compte (
	idCompte integer NOT NULL PRIMARY KEY,
	solde numeric(10,2) NOT NULL
);

CREATE TABLE Virement (
	idTransaction serial PRIMARY KEY,
	idCompteDebit integer NOT NULL,
	idCompteCredit integer NOT NULL,
	dateTransaction date NOT NULL DEFAULT current_date,
	montant numeric(10,2) NOT NULL
);

CREATE VIEW vComptes
AS
SELECT idCompte, solde
FROM Compte;

-- Jeu d'essai
INSERT INTO Compte (idCompte, solde) VALUES (1234567, 1000);
INSERT INTO Compte (idCompte, solde) VALUES (2345678, 2000);
INSERT INTO Compte (idCompte, solde) VALUES (3456789, 0);
