CREATE TABLE `accounts` (
	`address` BINARY(21) NOT NULL,
	`current_ledger_height` BIGINT NOT NULL,
	`is_declared` TINYINT(1) NOT NULL,

	PRIMARY KEY(`address`)
);

CREATE TABLE `balances` (
	`account` BINARY(21) NOT NULL,
	`currency` SMALLINT NOT NULL,
	`amount` BIGINT NOT NULL,

	PRIMARY KEY(`account`, `currency`)
);

CREATE TABLE `transactions` (
	`hash` BINARY(32) NOT NULL,
	`ledger_height` BIGINT NOT NULL,
	`expire` BIGINT NOT NULL,
	PRIMARY KEY(`hash`)
);

CREATE TABLE `transactioninputoutputs` (
	`transaction_hash` BINARY(32) NOT NULL,
	`index` TINYINT UNSIGNED NOT NULL,
	`is_input` TINYINT(1) NOT NULL,
	`account` BINARY(21) NOT NULL,
	`currency` SMALLINT NOT NULL,
	`amount` BIGINT NOT NULL,
	PRIMARY KEY(`transaction_hash`, `index`)
);

CREATE TABLE `transactionsignatures` (
	`transaction_hash` BINARY(32) NOT NULL,
	`publickey` BINARY(65) NOT NULL,
	`signature` BINARY(65) NOT NULL,
	PRIMARY KEY(`transaction_hash`,`publickey`)
);

CREATE TABLE `declarations` (
	`declaration_id` BIGINT NOT NULL,
	`type` TINYINT UNSIGNED NOT NULL,
	PRIMARY KEY(`declaration_id`)
);

CREATE TABLE `transactiondeclarations` (
    `transaction_hash` BINARY(32) NOT NULL,
	`index` TINYINT UNSIGNED NOT NULL,
	`declaration_id` BIGINT NOT NULL,
	PRIMARY KEY(`transaction_hash`, `index`)
);

CREATE TABLE `multisignatureaccounts` (
	`declaration_id` BIGINT NOT NULL,
	`account` BINARY(21) NOT NULL,
    `hash` BINARY(32) NOT NULL,
	`required` TINYINT NOT NULL,
	PRIMARY KEY(`declaration_id`),
	UNIQUE KEY(`account`)
);

CREATE TABLE `multisignaturesigners` (
	`multisignature_account` BINARY(21) NOT NULL,
	`signer` BINARY(21) NOT NULL,
	PRIMARY KEY(`multisignature_account`,`signer`)
);

CREATE TABLE `blocks` (
	`hash` BINARY(32) NOT NULL,
	`ledger_height` BIGINT NOT NULL,
	`fee_transaction_index` SMALLINT UNSIGNED NULL,
	PRIMARY KEY(`ledger_height`)
);

CREATE TABLE `transactionmessages` (
	`transaction_hash` BINARY(32) NOT NULL,
	`message` BINARY(10) NOT NULL,
	PRIMARY KEY(`transaction_hash`)
);

CREATE TABLE `tableledgerheights` (
	`table_name` VARCHAR(32) NOT NULL,
	`processed_ledger_height` BIGINT NOT NULL,
	PRIMARY KEY(`table_name`)
);


CREATE TABLE `hashlocks` (
    `declaration_id` BIGINT NOT NULL,
	`account` BINARY(21) NOT NULL,
	`secret_type` TINYINT UNSIGNED NOT NULL,
	`secret_hash` BINARY(32) NOT NULL,    
	PRIMARY KEY(`declaration_id`)
);

CREATE TABLE `timelocks` (
    `declaration_id` BIGINT NOT NULL,
	`account` BINARY(21) NOT NULL,
	`timestamp` BIGINT UNSIGNED NOT NULL,
	PRIMARY KEY(`declaration_id`),
	UNIQUE KEY(`account`)
);


CREATE TABLE `secretrevelations` (
    `declaration_id` BIGINT NOT NULL,
	`secret` BINARY(32) NOT NULL,
	PRIMARY KEY(`declaration_id`)
);