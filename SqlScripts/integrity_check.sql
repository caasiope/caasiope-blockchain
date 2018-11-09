-- check that input = outputs for each ledger
SELECT currency, ledger_height, SUM(IF(is_input = 0, -amount, amount)) AS total
FROM transactioninputoutputs
INNER JOIN transactions ON transactioninputoutputs.transaction_hash = transactions.hash
GROUP BY currency, ledger_height
HAVING (SUM(IF(is_input = 0, -amount, amount)) <> 0)
;

-- check that balances are equivalent
SELECT currency, SUM(amount) AS total
FROM balances
GROUP BY currency
;

-- check that issuers only have negative balances
SELECT *
FROM balances
LEFT JOIN issuers ON balances.account = issuers.account AND balances.currency = issuers.currency
WHERE amount < 0
AND issuers.account IS NULL
;

-- check that every ledger has a block
SELECT ledgers.height
FROM ledgers
LEFT JOIN blocks ON (ledgers.height = blocks.ledger_height)
WHERE ledgers.height IS NULL
;

-- check that every transaction has an input
SELECT *
FROM transactions WHERE transactions.hash NOT IN
(SELECT transaction_hash FROM transactioninputoutputs WHERE is_input = 1)
;

-- check that every transaction has an output
SELECT *
FROM transactions WHERE transactions.hash NOT IN
(SELECT transaction_hash FROM transactioninputoutputs WHERE is_input = 0)
;

-- check that every transaction is in a ledger
SELECT to_base64(transactions.hash)
FROM transactions
LEFT JOIN ledgers ON (ledgers.height = transactions.ledger_height)
WHERE ledgers.height IS NULL
;

-- check that every ledger has at least a transaction
SELECT ledgers.height
FROM ledgers
LEFT JOIN transactions ON (ledgers.height = transactions.ledger_height)
WHERE transactions.ledger_height IS NULL
;

-- check that every multisignature has a transaction
SELECT *
FROM multisignatureaccounts
LEFT JOIN transactions ON (multisignatureaccounts.transaction_hash = transactions.hash)
WHERE transactions.hash IS NULL
;

-- check that every multisignature has more signers than required
SELECT *
FROM multisignatureaccounts
WHERE required > (
	SELECT COUNT(*)
    FROM multisignaturesigners
    WHERE multisignatureaccounts.account = multisignaturesigners.multisignature_account)
OR required <= 0
;



