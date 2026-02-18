CREATE TABLE IF NOT EXISTS `LoyaltySettings` (
    `LoyaltySettingId` INT NOT NULL AUTO_INCREMENT,
    `PointsPerUnitCurrency` DECIMAL(18,4) NOT NULL DEFAULT 0,
    `RedemptionValuePerPoint` DECIMAL(18,4) NOT NULL DEFAULT 0,
    `MinimumRedeemPoints` INT NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL,
    PRIMARY KEY (`LoyaltySettingId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

