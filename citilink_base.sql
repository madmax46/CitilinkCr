--
-- Скрипт сгенерирован Devart dbForge Studio 2019 for MySQL, Версия 8.1.22.0
-- Домашняя страница продукта: http://www.devart.com/ru/dbforge/mysql/studio
-- Дата скрипта: 27.02.2020 4:28:41
-- Версия сервера: 5.5.5-10.4.11-MariaDB-1:10.4.11+maria~bionic
-- Версия клиента: 4.1
--

-- 
-- Отключение внешних ключей
-- 
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;

-- 
-- Установить режим SQL (SQL mode)
-- 
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- 
-- Установка кодировки, с использованием которой клиент будет посылать запросы на сервер
--
SET NAMES 'utf8';




--
-- Установка базы данных по умолчанию
--
USE IAS14_lyamoPV;

--
-- Создать таблицу `products_link`
--
CREATE TABLE IF NOT EXISTS products_link (
  id int(11) UNSIGNED NOT NULL AUTO_INCREMENT,
  idCategory int(11) UNSIGNED NOT NULL,
  link varchar(1000) NOT NULL,
  name varchar(255) NOT NULL,
  status tinyint(4) NOT NULL DEFAULT 0,
  last_update timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP () ON UPDATE CURRENT_TIMESTAMP,
  citilink_product_id int(11) UNSIGNED NOT NULL,
  PRIMARY KEY (id)
)
ENGINE = INNODB,
AUTO_INCREMENT = 1649,
AVG_ROW_LENGTH = 354,
CHARACTER SET utf8,
COLLATE utf8_general_ci;

--
-- Создать индекс `UK_products_link` для объекта типа таблица `products_link`
--
ALTER TABLE products_link
ADD UNIQUE INDEX UK_products_link (idCategory, link);

--
-- Создать таблицу `products_price_history`
--
CREATE TABLE IF NOT EXISTS products_price_history (
  id int(11) UNSIGNED NOT NULL AUTO_INCREMENT,
  productId int(11) UNSIGNED NOT NULL,
  price double NOT NULL,
  isInStock tinyint(4) NOT NULL,
  dateCheck timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP () ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id)
)
ENGINE = INNODB,
AUTO_INCREMENT = 134,
AVG_ROW_LENGTH = 123,
CHARACTER SET utf8,
COLLATE utf8_general_ci;

--
-- Создать индекс `IDX_products_price_history` для объекта типа таблица `products_price_history`
--
ALTER TABLE products_price_history
ADD INDEX IDX_products_price_history (productId, dateCheck);

--
-- Создать внешний ключ
--
ALTER TABLE products_price_history
ADD CONSTRAINT FK_products_price_history_productId FOREIGN KEY (productId)
REFERENCES products_link (id) ON DELETE NO ACTION;

--
-- Создать таблицу `characteristics`
--
CREATE TABLE IF NOT EXISTS characteristics (
  id int(11) UNSIGNED NOT NULL AUTO_INCREMENT,
  idGroup int(11) UNSIGNED NOT NULL,
  name varchar(500) NOT NULL,
  PRIMARY KEY (id)
)
ENGINE = INNODB,
AUTO_INCREMENT = 310,
AVG_ROW_LENGTH = 240,
CHARACTER SET utf8,
COLLATE utf8_general_ci;

--
-- Создать индекс `UK_characteristics` для объекта типа таблица `characteristics`
--
ALTER TABLE characteristics
ADD UNIQUE INDEX UK_characteristics (idGroup, name);

--
-- Создать таблицу `products_characteristics`
--
CREATE TABLE IF NOT EXISTS products_characteristics (
  id int(11) NOT NULL AUTO_INCREMENT,
  idProduct int(11) UNSIGNED NOT NULL,
  idCharacteristic int(11) UNSIGNED NOT NULL,
  value varchar(500) NOT NULL,
  PRIMARY KEY (id)
)
ENGINE = INNODB,
AUTO_INCREMENT = 5864,
AVG_ROW_LENGTH = 62,
CHARACTER SET utf8,
COLLATE utf8_general_ci;

--
-- Создать индекс `UK_products_characteristics` для объекта типа таблица `products_characteristics`
--
ALTER TABLE products_characteristics
ADD UNIQUE INDEX UK_products_characteristics (idProduct, idCharacteristic);

--
-- Создать внешний ключ
--
ALTER TABLE products_characteristics
ADD CONSTRAINT FK_products_characteristics_idCharacteristic FOREIGN KEY (idCharacteristic)
REFERENCES characteristics (id) ON DELETE NO ACTION;

--
-- Создать внешний ключ
--
ALTER TABLE products_characteristics
ADD CONSTRAINT FK_products_characteristics_idProduct FOREIGN KEY (idProduct)
REFERENCES products_link (id) ON DELETE NO ACTION;

--
-- Создать таблицу `proxy_list`
--
CREATE TABLE IF NOT EXISTS proxy_list (
  proxy varchar(255) DEFAULT NULL
)
ENGINE = INNODB,
AVG_ROW_LENGTH = 51,
CHARACTER SET utf8,
COLLATE utf8_general_ci;

--
-- Создать индекс `UK_proxy_list_proxy` для объекта типа таблица `proxy_list`
--
ALTER TABLE proxy_list
ADD UNIQUE INDEX UK_proxy_list_proxy (proxy);

--
-- Создать таблицу `characteristics_groups`
--
CREATE TABLE IF NOT EXISTS characteristics_groups (
  id int(11) NOT NULL AUTO_INCREMENT,
  name varchar(500) NOT NULL,
  PRIMARY KEY (id, name)
)
ENGINE = INNODB,
AUTO_INCREMENT = 202,
AVG_ROW_LENGTH = 1260,
CHARACTER SET utf8,
COLLATE utf8_general_ci;

--
-- Создать индекс `UK_characteristics_groups_name` для объекта типа таблица `characteristics_groups`
--
ALTER TABLE characteristics_groups
ADD UNIQUE INDEX UK_characteristics_groups_name (name);

--
-- Создать таблицу `categories`
--
CREATE TABLE IF NOT EXISTS categories (
  id int(11) UNSIGNED NOT NULL AUTO_INCREMENT,
  name varchar(255) DEFAULT NULL,
  link varchar(1000) NOT NULL,
  parse_status tinyint(4) NOT NULL DEFAULT 0,
  last_update timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP (),
  pages_num int(11) UNSIGNED NOT NULL DEFAULT 0,
  last_parsed_page int(11) UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (id)
)
ENGINE = INNODB,
AUTO_INCREMENT = 95,
AVG_ROW_LENGTH = 178,
CHARACTER SET utf8,
COLLATE utf8_general_ci;

--
-- Создать индекс `UK_categories_link` для объекта типа таблица `categories`
--
ALTER TABLE categories
ADD UNIQUE INDEX UK_categories_link (link);

-- 
-- Восстановить предыдущий режим SQL (SQL mode)
-- 
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;

-- 
-- Включение внешних ключей
-- 
/*!40014 SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS */;