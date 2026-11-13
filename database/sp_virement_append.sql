CREATE OR REPLACE FUNCTION sp_virement_append(pIdCompteDebit integer,
                                              pIdCompteCredit integer,
                                              pMontant numeric)
RETURNS void
AS $$
BEGIN
	UPDATE compte SET solde = solde - pMontant WHERE idcompte = pIdCompteDebit;
	UPDATE compte SET solde = solde + pMontant WHERE idcompte = pIdCompteCredit;
	INSERT INTO virement(idcomptedebit, idcomptecredit, datetransaction, montant)
		VALUES (pIdCompteDebit, pIdCompteCredit, current_date, pMontant);
EXCEPTION
	WHEN others THEN
		RAISE EXCEPTION 'Erreur fonction stockée sp_virement_append';
END;
$$ LANGUAGE plpgsql;

-- Test
-- SELECT sp_virement_append(1234567, 2345678, 100.00);
-- SELECT * FROM virement;
