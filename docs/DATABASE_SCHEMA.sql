-- ============================================================================
-- Sevro – Local Service Marketplace
-- MySQL / MariaDB (XAMPP) Reference Schema (Matches EF Core MySQL Migrations)
-- NOTE: EF Core automatically creates this schema via:
--   dotnet ef database update
-- ============================================================================

CREATE DATABASE IF NOT EXISTS `SevroDb` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `SevroDb`;

-- 1. ServiceCategories
CREATE TABLE `ServiceCategories` (
    `CategoryId`   INT          NOT NULL AUTO_INCREMENT,
    `CategoryName` VARCHAR(100) NOT NULL,
    `Description`  VARCHAR(500) NULL,
    PRIMARY KEY (`CategoryId`),
    UNIQUE KEY `IX_ServiceCategories_CategoryName` (`CategoryName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. ProviderProfiles (1:1 with AspNetUsers)
CREATE TABLE `ProviderProfiles` (
    `ProviderProfileId` INT           NOT NULL AUTO_INCREMENT,
    `UserId`            VARCHAR(255)  NOT NULL,
    `BusinessName`      VARCHAR(150)  NOT NULL,
    `ExperienceYears`   INT           NOT NULL,
    `Description`       VARCHAR(1000) NULL,
    `City`              VARCHAR(100)  NOT NULL,
    `IsVerified`        TINYINT(1)    NOT NULL DEFAULT 0,
    `IsAvailable`       TINYINT(1)    NOT NULL DEFAULT 1,
    PRIMARY KEY (`ProviderProfileId`),
    UNIQUE KEY `IX_ProviderProfiles_UserId` (`UserId`),
    CONSTRAINT `FK_ProviderProfiles_AspNetUsers_UserId`
        FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. ProviderServices
CREATE TABLE `ProviderServices` (
    `ProviderServiceId` INT            NOT NULL AUTO_INCREMENT,
    `ProviderProfileId` INT            NOT NULL,
    `CategoryId`        INT            NOT NULL,
    `ServiceName`       VARCHAR(150)   NOT NULL,
    `Description`       VARCHAR(1000)  NULL,
    `Price`             DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (`ProviderServiceId`),
    CONSTRAINT `FK_ProviderServices_ProviderProfiles`
        FOREIGN KEY (`ProviderProfileId`) REFERENCES `ProviderProfiles` (`ProviderProfileId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ProviderServices_ServiceCategories`
        FOREIGN KEY (`CategoryId`) REFERENCES `ServiceCategories` (`CategoryId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. CustomerAddresses
CREATE TABLE `CustomerAddresses` (
    `AddressId`  INT          NOT NULL AUTO_INCREMENT,
    `CustomerId` VARCHAR(255) NOT NULL,
    `Address`    VARCHAR(300) NOT NULL,
    `City`       VARCHAR(100) NOT NULL,
    `Pincode`    VARCHAR(10)  NOT NULL,
    PRIMARY KEY (`AddressId`),
    CONSTRAINT `FK_CustomerAddresses_AspNetUsers`
        FOREIGN KEY (`CustomerId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. Bookings (Status: Pending, Confirmed, Rejected, InProgress, Completed, Cancelled)
CREATE TABLE `Bookings` (
    `BookingId`         INT          NOT NULL AUTO_INCREMENT,
    `CustomerId`        VARCHAR(255) NOT NULL,
    `ProviderServiceId` INT          NOT NULL,
    `AddressId`         INT          NOT NULL,
    `BookingDate`       DATE         NOT NULL,
    `Status`            VARCHAR(20)  NOT NULL,
    `CreatedAt`         DATETIME(6)  NOT NULL,
    PRIMARY KEY (`BookingId`),
    CONSTRAINT `FK_Bookings_AspNetUsers`
        FOREIGN KEY (`CustomerId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Bookings_ProviderServices`
        FOREIGN KEY (`ProviderServiceId`) REFERENCES `ProviderServices` (`ProviderServiceId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Bookings_CustomerAddresses`
        FOREIGN KEY (`AddressId`) REFERENCES `CustomerAddresses` (`AddressId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. Reviews (1:1 optional with Bookings, Rating 1-5)
CREATE TABLE `Reviews` (
    `ReviewId`   INT           NOT NULL AUTO_INCREMENT,
    `BookingId`  INT           NOT NULL,
    `CustomerId` VARCHAR(255)  NOT NULL,
    `Rating`     INT           NOT NULL,
    `Comment`    VARCHAR(1000) NULL,
    `CreatedAt`  DATETIME(6)   NOT NULL,
    PRIMARY KEY (`ReviewId`),
    UNIQUE KEY `IX_Reviews_BookingId` (`BookingId`),
    CONSTRAINT `CK_Reviews_Rating` CHECK (`Rating` >= 1 AND `Rating` <= 5),
    CONSTRAINT `FK_Reviews_Bookings`
        FOREIGN KEY (`BookingId`) REFERENCES `Bookings` (`BookingId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reviews_AspNetUsers`
        FOREIGN KEY (`CustomerId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
