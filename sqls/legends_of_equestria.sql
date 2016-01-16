-- --------------------------------------------------------
-- Хост:                         127.0.0.1
-- Версия сервера:               5.6.17 - MySQL Community Server (GPL)
-- ОС Сервера:                   Win64
-- HeidiSQL Версия:              9.3.0.4984
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Дамп структуры базы данных legends_of_equestria
CREATE DATABASE IF NOT EXISTS `legends_of_equestria` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `legends_of_equestria`;


-- Дамп структуры для таблица legends_of_equestria.loe_character
CREATE TABLE IF NOT EXISTS `loe_character` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user` int(11) NOT NULL,
  `level` smallint(6) NOT NULL,
  `map` int(11) NOT NULL,
  `data` longblob,
  `name` varchar(64) NOT NULL,
  `race` tinyint(3) unsigned NOT NULL,
  `gender` tinyint(3) unsigned NOT NULL,
  `eye` smallint(6) NOT NULL,
  `tail` smallint(6) NOT NULL,
  `hoof` smallint(6) NOT NULL,
  `mane` smallint(6) NOT NULL,
  `bodysize` float NOT NULL,
  `hornsize` float NOT NULL,
  `eyecolor` int(11) NOT NULL,
  `hoofcolor` int(11) NOT NULL,
  `bodycolor` int(11) NOT NULL,
  `haircolor0` int(11) NOT NULL,
  `haircolor1` int(11) NOT NULL,
  `haircolor2` int(11) NOT NULL,
  `cutiemark0` int(11) NOT NULL,
  `cutiemark1` int(11) NOT NULL,
  `cutiemark2` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `CHAR NAME` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_character: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `loe_character` DISABLE KEYS */;
/*!40000 ALTER TABLE `loe_character` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_creature
CREATE TABLE IF NOT EXISTS `loe_creature` (
  `id` int(11) NOT NULL,
  `loot` int(11) NOT NULL DEFAULT '-1',
  `flags` tinyint(3) unsigned NOT NULL,
  `spell` int(11) NOT NULL DEFAULT '-1',
  `speed` float NOT NULL,
  `resource` int(11) NOT NULL,
  `kill_credit` smallint(5) unsigned NOT NULL,
  `attack_rate` float NOT NULL,
  `base_resist` float NOT NULL,
  `base_armor` float NOT NULL,
  `base_dodge` float NOT NULL,
  `base_power` float NOT NULL,
  `base_health` float NOT NULL,
  `base_energy` float NOT NULL,
  `base_hp_reg` float NOT NULL,
  `base_ep_reg` float NOT NULL,
  `base_dmg_min` float NOT NULL,
  `base_dmg_max` float NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_creature: ~4 rows (приблизительно)
/*!40000 ALTER TABLE `loe_creature` DISABLE KEYS */;
INSERT INTO `loe_creature` (`id`, `loot`, `flags`, `spell`, `speed`, `resource`, `kill_credit`, `attack_rate`, `base_resist`, `base_armor`, `base_dodge`, `base_power`, `base_health`, `base_energy`, `base_hp_reg`, `base_ep_reg`, `base_dmg_min`, `base_dmg_max`) VALUES
	(1, -1, 0, -1, 350, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
INSERT INTO `loe_creature` (`id`, `loot`, `flags`, `spell`, `speed`, `resource`, `kill_credit`, `attack_rate`, `base_resist`, `base_armor`, `base_dodge`, `base_power`, `base_health`, `base_energy`, `base_hp_reg`, `base_ep_reg`, `base_dmg_min`, `base_dmg_max`) VALUES
	(2, -1, 0, -1, 350, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
INSERT INTO `loe_creature` (`id`, `loot`, `flags`, `spell`, `speed`, `resource`, `kill_credit`, `attack_rate`, `base_resist`, `base_armor`, `base_dodge`, `base_power`, `base_health`, `base_energy`, `base_hp_reg`, `base_ep_reg`, `base_dmg_min`, `base_dmg_max`) VALUES
	(3, -1, 0, -1, 350, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
INSERT INTO `loe_creature` (`id`, `loot`, `flags`, `spell`, `speed`, `resource`, `kill_credit`, `attack_rate`, `base_resist`, `base_armor`, `base_dodge`, `base_power`, `base_health`, `base_energy`, `base_hp_reg`, `base_ep_reg`, `base_dmg_min`, `base_dmg_max`) VALUES
	(4, 1, 0, 6, 325, 15, 0, 1500, 3, 4, 0, 20, 250, 0, 0.5, 0, 15, 25);
INSERT INTO `loe_creature` (`id`, `loot`, `flags`, `spell`, `speed`, `resource`, `kill_credit`, `attack_rate`, `base_resist`, `base_armor`, `base_dodge`, `base_power`, `base_health`, `base_energy`, `base_hp_reg`, `base_ep_reg`, `base_dmg_min`, `base_dmg_max`) VALUES
	(5, -1, 0, 17, 400, 12, 0, 2500, 5.5, 5.5, 0, 65, 666, 0, 1, 0, 200, 280);
/*!40000 ALTER TABLE `loe_creature` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_dialog
CREATE TABLE IF NOT EXISTS `loe_dialog` (
  `id` smallint(5) unsigned NOT NULL AUTO_INCREMENT,
  `state` smallint(6) NOT NULL DEFAULT '-1',
  `npc` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `message` int(11) NOT NULL DEFAULT '-1',
  `condition` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `cndData01` int(11) NOT NULL DEFAULT '-1',
  `cndData02` int(11) NOT NULL DEFAULT '-1',
  `command` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `cmdData01` int(11) NOT NULL DEFAULT '-1',
  `cmdData02` int(11) NOT NULL DEFAULT '-1',
  PRIMARY KEY (`id`,`state`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_dialog: ~65 rows (приблизительно)
/*!40000 ALTER TABLE `loe_dialog` DISABLE KEYS */;
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, -1, 0, 255, -1, 35, 6, -1, 2, 0, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 0, 0, 0, 1, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 1, 0, 0, 2, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 2, 0, 254, 3, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 3, 0, 254, 4, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 4, 0, 254, 5, 0, -1, -1, 2, 8, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 5, 0, 254, 6, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 6, 0, 0, 7, 0, -1, -1, 1, 7, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 7, 0, 0, 8, 0, -1, -1, 1, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 8, 0, 255, -1, 0, -1, -1, 3, 42000, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 9, 0, 255, -1, 0, -1, -1, 4, 42000, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 10, 0, 0, 9, 0, -1, -1, 1, 11, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(1, 11, 0, 0, 10, 0, -1, -1, 1, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, -1, 0, 255, -1, 35, -1, -1, 2, 0, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 0, 0, 0, 11, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 1, 1, 0, 12, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 2, 0, 0, 13, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 3, 1, 0, 14, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 4, 0, 0, 15, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 5, 1, 0, 16, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 6, 0, 0, 17, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 7, 1, 0, 18, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 8, 0, 0, 19, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 9, 1, 0, 20, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 10, 0, 0, 21, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 11, 1, 0, 22, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 12, 0, 0, 23, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 13, 0, 254, 24, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 14, 0, 0, 25, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 15, 1, 253, 30, 0, -1, -1, 1, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 16, 0, 253, 29, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 17, 0, 254, 26, 0, -1, -1, 2, 20, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 18, 0, 254, 27, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 19, 0, 0, 28, 0, -1, -1, 1, 15, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 20, 0, 0, 31, 0, -1, -1, 1, 21, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 21, 1, 253, 30, 0, -1, -1, 1, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 22, 0, 253, 32, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 23, 0, 254, 33, 7, 69, 4, 2, 26, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 24, 0, 254, 34, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 25, 0, 0, 28, 0, -1, -1, 1, 21, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 26, 0, 255, -1, 0, -1, -1, 3, 10000, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 27, 0, 255, -1, 0, -1, -1, 4, 10000, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 28, 0, 255, -1, 0, -1, -1, 7, 69, 4);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 29, 0, 0, 35, 0, -1, -1, 1, 30, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 30, 1, 253, 37, 0, -1, -1, 1, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(2, 31, 0, 253, 36, 0, -1, -1, 1, 30, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, -2, 0, 255, -1, 82, 1, -1, 2, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, -1, 0, 255, -1, 51, 1, -1, 2, 1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 0, 0, 0, 38, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 1, 0, 253, 48, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 2, 0, 254, 33, 0, -1, -1, 2, 5, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 3, 0, 254, 34, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 4, 0, 0, 45, 0, -1, -1, 1, 1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 5, 0, 0, 49, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 6, 0, 255, -1, 0, -1, -1, 32, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 7, 0, 255, -1, 0, -1, -1, 33, 1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 8, 0, 255, -1, 0, -1, -1, 1, 9, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 9, 0, 255, -1, 82, 16, -1, 1, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 10, 0, 0, 39, 0, -1, -1, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 11, 0, 254, 40, 0, -1, -1, 2, 22, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 12, 0, 254, 41, 7, -1, 10, 0, -1, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 13, 0, 254, 42, 7, -1, 100, 2, 18, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 14, 0, 254, 43, 0, -1, -1, 2, 21, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 15, 0, 255, -1, 0, -1, -1, 5, 3, 1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 16, 0, 255, -1, 0, -1, -1, 6, 10, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 17, 0, 255, -1, 0, -1, -1, 2, 20, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 18, 0, 255, -1, 0, -1, -1, 5, 3, 10);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 19, 0, 255, -1, 0, -1, -1, 6, 100, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 20, 0, 0, 46, 0, -1, -1, 2, 11, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 21, 0, 0, 45, 0, -1, -1, 1, 10, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 22, 0, 0, 44, 0, -1, -1, 1, 23, -1);
INSERT INTO `loe_dialog` (`id`, `state`, `npc`, `type`, `message`, `condition`, `cndData01`, `cndData02`, `command`, `cmdData01`, `cmdData02`) VALUES
	(3, 23, 0, 0, 47, 0, -1, -1, 1, -1, -1);
/*!40000 ALTER TABLE `loe_dialog` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_item
CREATE TABLE IF NOT EXISTS `loe_item` (
  `id` int(11) NOT NULL,
  `name` varchar(64) DEFAULT NULL,
  `flags` tinyint(3) unsigned NOT NULL,
  `level` tinyint(3) unsigned NOT NULL,
  `stack` tinyint(3) unsigned NOT NULL,
  `price` smallint(5) unsigned NOT NULL,
  `slot` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_item: ~296 rows (приблизительно)
/*!40000 ALTER TABLE `loe_item` DISABLE KEYS */;
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(0, 'Green Apple', 3, 1, 10, 3, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(1, 'Red apple', 3, 1, 10, 3, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(2, 'Yellow Apple', 3, 1, 10, 3, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(3, 'Banana', 3, 1, 15, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(4, 'Carrot', 3, 1, 20, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(5, 'Celery Stalk', 3, 1, 20, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(6, 'Cookie', 3, 1, 20, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(7, 'Cupcake', 3, 1, 8, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(8, 'Pear', 3, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(9, 'Flute', 2, 1, 0, 25, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(10, 'Saxophone', 2, 1, 0, 35, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(11, 'Trombone', 2, 1, 0, 30, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(12, 'Trumpet', 2, 1, 0, 23, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(13, 'Violin', 2, 1, 0, 40, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(14, 'Chunk of Ore', 3, 1, 8, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(16, 'Book Hat', 2, 1, 0, 8, -2147483648);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(17, 'Goggles', 2, 1, 0, 6, 1024);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(18, 'Glasses', 2, 1, 0, 6, 1024);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(19, 'Cowpony Hat', 2, 1, 0, 6, -2147483648);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(20, 'Potion Bags', 2, 1, 0, 6, 1073741824);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(21, 'Lab Coat', 2, 1, 0, 6, 128);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(22, 'Boots', 2, 1, 0, 6, 48);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(23, 'Gala boots', 2, 1, 1, 20, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(24, 'Pickaxe', 2, 1, 1, 7, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(25, 'Hammer', 2, 1, 10, 8, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(26, 'Strawberry ', 3, 1, 10, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(27, 'Strawberry Torte', 3, 1, 10, 8, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(28, 'Medical Potion Crate', 1, 1, 10, 10, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(29, 'Stack of Cloth', 1, 1, 10, 4, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(30, 'Food Box', 1, 1, 10, 10, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(31, 'Assistant Nurse Cap', 3, 1, 10, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(32, 'Rainbow Essence', 3, 1, 10, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(33, 'Hard Hat ', 2, 1, 10, 7, -2147483648);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(34, 'Health Potion', 3, 1, 30, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(35, 'Mixing Spoon', 3, 1, 10, 2, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(36, 'Torch', 3, 1, 10, 3, 512);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(37, 'Water Bucket', 3, 1, 10, 5, 512);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(38, 'Water Bucket (Full)', 2, 1, 10, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(39, 'Frozen Essence Drops', 3, 1, 50, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(40, 'Musketeer Hat', 3, 1, 10, 15, -2147483648);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(41, 'Egg', 3, 1, 30, 2, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(42, 'Skyhammer\'s Book ', 1, 1, 5, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(43, 'Whipped Cream Pie ', 3, 1, 10, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(44, 'Apple Pie', 3, 1, 10, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(45, 'Gooseberry Pie', 3, 1, 10, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(46, 'Banoffee Pie', 3, 1, 10, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(47, 'Strawberry Pie ', 3, 1, 10, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(48, 'Raspberry Pie', 3, 1, 10, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(49, 'Cherry Pie ', 3, 1, 10, 6, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(50, 'Blueberry Pie', 3, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(51, 'Empty Pie Tin ', 3, 1, 20, 2, -2147483648);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(52, 'General Messerschmitt\'s Speech ', 0, 1, 1, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(53, 'Messerchmitt\'s Letter of Recommendation', 0, 1, 1, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(54, 'Bronze Back Legs', 6, 1, 10, 50, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(55, 'Bronze Helmet', 6, 1, 10, 75, 1024);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(56, 'Bronze Chest Plate', 6, 1, 10, 100, 128);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(57, 'Bronze Front Legs', 6, 1, 10, 50, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(58, 'Lab Goggles', 2, 1, 10, 6, 1024);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(59, 'Scroll Bag', 3, 1, 10, 6, 1073741824);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(60, 'Satchel Bag', 2, 1, 10, 6, 1073741824);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(61, 'Wooden Sword', 2, 1, 10, 5, 1073741824);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(62, 'Ellowee\'s Hat', 0, 1, 10, 10, -2147483648);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(63, 'Dandelion Bracelet', 2, 1, 10, 5, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(64, 'Dandelion', 3, 1, 50, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(65, 'Fluffle Puff', 0, 1, 10, 666, 176);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(66, 'Fluffle Hat', 2, 1, 10, 600, -2147483648);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(67, 'Bronze Back Plate', 6, 1, 10, 75, 1073741824);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(68, 'Mushrooms', 3, 1, 10, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(69, 'Mountain Flower', 3, 1, 10, 3, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(70, 'Smooth Rock', 3, 1, 30, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(71, 'Ruby', 3, 1, 20, 7, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(72, 'Ruby Dust', 3, 1, 20, 4, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(73, 'Raincloud Hat', 2, 1, 10, 15, -2147483648);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(74, 'Big Package', 2, 1, 1, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(75, 'Paso Fino Basket', 2, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(76, 'Cirrus\'s Diary', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(77, 'Scribble\'s Novel', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(78, 'Hullabaloo\'s Headphones', 0, 1, 10, 800, 256);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(79, 'Bugsy\'s Bug', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(81, 'Rabbit Egg', 8, 1, 10, 900, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(82, 'Dog Egg', 8, 1, 10, 900, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(83, 'Amphora Base', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(84, 'Amphora Statuette', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(85, 'Amphora Grip', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(86, 'Without Any Seam or Needlework', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(87, 'Lantern', 2, 1, 10, 150, 512);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(88, 'Socks', 3, 1, 10, 50, 48);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(89, 'Medical Supplies', 0, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(90, 'Frying Pan', 3, 1, 5, 5, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(91, 'Pyrite\'s Package', 0, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(92, 'Midnight\'s Package', 0, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(93, 'Cat Egg', 8, 1, 10, 900, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(94, '"Sand to Stardom" Prerelease Copy', 2, 1, 10, 20, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(95, 'Catalepsy, Signed 1st Edition', 2, 1, 10, 20, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(96, 'Rock Candy', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(97, 'Granny Grey\'s Apple Pie', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(98, 'Farmer Fields\' Gooseberry Pie', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(99, 'Gomi\'s Cherry Pie', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(100, 'Buck\'s Whipped Cream Pie', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(101, 'Looking Glass', 0, 1, 10, 100, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(102, 'Moonlight Back Legs', 6, 50, 10, 4300, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(103, 'Moonlight Back Plate', 6, 50, 10, 4400, 1073741824);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(104, 'Moonlight Chest Plate', 6, 50, 10, 4400, 128);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(105, 'Moonlight Front Legs', 6, 50, 10, 4300, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(106, 'Moonlight Helmet', 6, 50, 10, 4450, 1024);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(107, 'Gold Back Legs', 6, 30, 10, 600, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(108, 'Gold Back Plate', 6, 30, 10, 800, 1073741824);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(109, 'Gold Chest Plate', 6, 30, 10, 800, 128);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(110, 'Gold Front Legs', 7, 30, 10, 600, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(111, 'Gold Helmet', 6, 30, 10, 900, 1024);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(112, 'Iron Back Legs', 6, 10, 10, 300, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(113, 'Iron Back Plate', 7, 10, 10, 350, 1073741824);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(114, 'Iron Chest Plate', 6, 10, 10, 350, 128);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(115, 'Iron Front Legs', 6, 10, 10, 300, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(116, 'Iron Helmet', 6, 10, 10, 350, 1024);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(117, 'Scribble\'s Paper', 1, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(118, 'Vogue\'s Report', 0, 1, 10, 0, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(119, 'Sunlight Back Legs', 6, 50, 10, 4300, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(120, 'Sunlight Back Plate', 6, 50, 10, 4400, 1073741824);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(121, 'Sunlight Chest Plate', 6, 50, 10, 4400, 128);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(122, 'Sunlight Front Legs', 6, 50, 10, 4300, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(123, 'Sunlight Helmet', 6, 50, 10, 4450, 1024);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(124, 'Test Helmet', 6, 25, 10, 999, 1024);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(125, 'Visa\'s Note', 1, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(126, 'Argyle Socks (Back Legs)', 3, 1, 10, 50, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(127, 'Argyle Socks (Front Legs)', 3, 1, 10, 50, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(128, 'Checker Socks (Back Legs)', 3, 1, 10, 50, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(129, 'Checker Socks (Front Legs)', 3, 1, 10, 50, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(130, 'Dot Socks (Back Legs)', 3, 1, 10, 50, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(131, 'Dot Socks (Front Legs)', 3, 1, 10, 50, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(132, 'Horizonal Striped Socks (Back Legs)', 3, 1, 10, 50, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(133, 'Horizonal Striped Socks (Front Legs)', 3, 1, 10, 50, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(134, 'Tartan Socks (Back Legs)', 3, 1, 10, 50, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(135, 'Tartan Socks (Front Legs)', 3, 1, 10, 50, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(136, 'Vertical Striped Socks (Back Legs)', 3, 1, 10, 50, 32);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(137, 'Vertical Striped Socks (Front Legs)', 3, 1, 10, 50, 16);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(138, 'Arrest Warrant', 0, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(139, 'Stopping Salt', 0, 1, 10, 200, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(140, 'Gems', 3, 1, 10, 30, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(141, 'Hornet\'s Nest', 1, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(142, 'Linen Scrap', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(143, 'Canvas Scrap', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(144, 'Temporal Scrap', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(145, 'Copper Ingot', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(146, 'Bronze Ingot', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(147, 'Iron Ingot', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(148, 'Steel Ingot', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(149, 'Alicium Ingot', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(150, 'Titanium Ingot', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(151, 'Elementium Ingot', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(152, 'Spacial Ingot', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(153, 'Heating Catalyst', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(154, 'Magical Catalyst', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(155, 'Pouch Template', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(156, 'Linen Strap', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(157, 'Linen Padding', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(158, 'Linen Square', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(159, 'Canvas Strap', 3, 20, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(160, 'Canvas Padding', 3, 20, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(162, 'Canvas Square', 3, 20, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(163, 'Woven Strap', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(164, 'Woven Padding', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(165, 'Temporal Strip', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(166, 'Temporal Padding', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(167, 'Temporal Square', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(168, 'Copper Strip', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(169, 'Copper Sheet', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(170, 'Copper Casing', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(171, 'Bronze Strip', 3, 10, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(172, 'Bronze Sheet', 3, 10, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(173, 'Bronze Casing', 3, 10, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(174, 'Iton Strip', 3, 20, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(175, 'Iron Sheet', 3, 20, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(176, 'Iron Casing', 3, 20, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(177, 'Steel Strip', 3, 30, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(178, 'Steel Sheet', 3, 30, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(179, 'Steel Casing', 3, 30, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(180, 'Alicium Strip', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(181, 'Alicium Sheet', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(182, 'Alicium Casing', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(183, 'Titanium Strip', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(184, 'Titanium Sheet', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(185, 'Titanium Casing', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(186, 'Elementium Strip', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(187, 'Elementium Sheet', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(188, 'Elementium Casing', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(189, 'Spacial Cast', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(190, 'Copper Back Plate', 3, 3, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(191, 'Copper Chest Plate', 3, 6, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(192, 'Copper Helmet', 3, 9, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(193, 'Bronze Back Plate', 3, 12, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(194, 'Bronze Chest Plate', 3, 14, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(195, 'Bronze Front Legs', 3, 16, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(196, 'Bronze Helmet', 3, 18, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(197, 'Iron Back Legs', 3, 21, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(198, 'Iron Back Plate', 3, 23, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(199, 'Iron Chest Plate', 3, 25, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(200, 'Iron Front Legs', 3, 27, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(201, 'Iron Helmet', 3, 29, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(202, 'Steel Back Legs', 3, 31, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(203, 'Steel Back Plate', 3, 33, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(204, 'Steel Chest Plate', 3, 35, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(205, 'Steel Front Legs', 3, 37, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(206, 'Steel Helmet', 3, 39, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(207, 'Alicium Back Legs', 3, 41, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(208, 'Alicium Back Plate', 3, 43, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(209, 'Alicium Chest Plate', 3, 45, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(210, 'Alicium Front Legs', 3, 47, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(211, 'Alicium Helmet', 3, 49, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(212, 'Titanium Back legs', 3, 41, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(213, 'Titanium Back Plate', 3, 43, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(214, 'Titanium Chest Plate', 3, 45, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(215, 'Titanium Front Legs', 3, 47, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(216, 'Titanium Helmet', 3, 49, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(217, 'Elementium Back Legs', 3, 41, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(218, 'Elementium Back Plate', 3, 43, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(219, 'Elementium Chest Plate', 3, 45, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(220, 'Elementium Front Legs', 3, 47, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(221, 'Elementium Helmet', 3, 49, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(222, 'Harmonious Back Legs', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(223, 'Harmonious Back Plate', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(224, 'Harmonious Chest Plate', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(225, 'Harmonious Front Legs', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(226, 'Harmonious Helmet', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(227, 'Mage Cloak', 3, 3, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(228, 'Sanguine Back Legs', 3, 4, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(229, 'Sanguine Back Plate', 3, 4, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(230, 'Sanguine Chest Plate', 3, 4, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(231, 'Sanguine Front Legs', 3, 4, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(232, 'Sanguine Helmet', 3, 4, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(233, 'Linen Pouch', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(234, 'Canvas Pouch', 3, 20, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(235, 'Woven Pouch', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(236, 'Amor Harmony 1', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(237, 'Amor Harmony 2', 3, 10, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(238, 'Amor Harmony 3', 3, 20, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(239, 'Amor Harmony 4', 3, 30, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(240, 'Amor Harmony 5', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(241, 'Amor Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(242, 'Magic Resist Harmony 1', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(243, 'Magic Resist Harmony 2', 3, 10, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(244, 'Magic Resist Harmony 3', 3, 20, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(245, 'Magic Resist Harmony 4', 3, 30, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(246, 'Magic Resist Harmony 5', 3, 40, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(247, 'Magic Resist Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(248, 'Health Harmony 1', 3, 2, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(249, 'Health Harmony 2', 3, 12, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(250, 'Health Harmony 3', 3, 22, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(251, 'Health Harmony 4', 3, 32, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(252, 'Health Harmony 5', 3, 42, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(253, 'Health Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(254, 'Health Regen Harmony 1', 3, 2, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(255, 'Health Regen Harmony 2', 3, 12, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(256, 'Health Regen Harmony 3', 3, 22, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(257, 'Health Regen Harmony 4', 3, 32, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(258, 'Health Regen Harmony 5', 3, 42, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(259, 'Health Regen Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(260, 'Energy Harmony 1', 3, 4, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(261, 'Energy Harmony 2', 3, 14, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(262, 'Energy Harmony 3', 3, 24, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(263, 'Energy Harmony 4', 3, 34, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(264, 'Energy Harmony 5', 3, 44, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(265, 'Energy Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(266, 'Energy Regen Harmony 1', 3, 4, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(267, 'Energy Regen Harmony 2', 3, 14, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(268, 'Energy Regen Harmony 3', 3, 24, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(269, 'Energy Regen Harmony 4', 3, 34, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(270, 'Energy Regen Harmony 5', 3, 44, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(271, 'Energy Regen Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(272, 'Movement Speed Harmony 1', 3, 6, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(273, 'Movement Speed Harmony 2', 3, 16, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(274, 'Movement Speed Harmony 3', 3, 26, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(275, 'Movement Speed Harmony 4', 3, 36, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(276, 'Movement Speed Harmony 5', 3, 46, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(277, 'Movement Speed Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(278, 'Ability Damage Harmony 1', 3, 8, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(279, 'Ability Damage Harmony 2', 3, 18, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(280, 'Ability Damage Harmony 3', 3, 28, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(281, 'Ability Damage Harmony 4', 3, 38, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(282, 'Ability Damage Harmony 5', 3, 48, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(283, 'Ability Damage Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(284, 'Ability Healing Harmony 1', 3, 8, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(285, 'Ability Healing Harmony 2', 3, 18, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(286, 'Ability Healing Harmony 3', 3, 28, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(287, 'Ability Healing Harmony 4', 3, 38, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(288, 'Ability Healing Harmony 5', 3, 48, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(289, 'Ability Healing Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(290, 'Payback Harmony 4', 3, 38, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(291, 'Payback Harmony 5', 3, 48, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(292, 'Payback Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(293, 'Starlight Harmony 4', 3, 38, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(294, 'Starlight Harmony 5', 3, 48, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(295, 'Starlight Harmony 6', 3, 50, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(296, 'Silky Roads\' Letter', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(297, 'Lavender Bloom\'s Letter', 3, 1, 10, 1, 0);
INSERT INTO `loe_item` (`id`, `name`, `flags`, `level`, `stack`, `price`, `slot`) VALUES
	(298, 'Sempai Dress', 2, 1, 10, 20, 8320);
/*!40000 ALTER TABLE `loe_item` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_item_stat
CREATE TABLE IF NOT EXISTS `loe_item_stat` (
  `id` int(11) NOT NULL,
  `stat` tinyint(3) unsigned NOT NULL,
  `value` int(11) NOT NULL,
  PRIMARY KEY (`id`,`stat`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_item_stat: ~141 rows (приблизительно)
/*!40000 ALTER TABLE `loe_item_stat` DISABLE KEYS */;
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(54, 1, 33);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(54, 2, 3);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(54, 3, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(54, 7, 4);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(54, 8, 3);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(55, 1, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(55, 2, 8);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(55, 3, 25);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(55, 7, 4);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(55, 8, 3);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(56, 1, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(56, 2, 8);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(56, 3, 25);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(56, 7, 4);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(56, 8, 3);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(57, 1, 33);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(57, 2, 3);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(57, 3, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(57, 7, 4);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(57, 8, 3);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(67, 1, 40);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(67, 2, 3);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(67, 3, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(67, 7, 4);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(67, 8, 3);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(102, 1, 666);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(102, 2, 66);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(102, 3, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(102, 7, 75);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(102, 8, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(103, 1, 800);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(103, 2, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(103, 3, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(103, 7, 75);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(103, 8, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(104, 1, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(104, 2, 150);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(104, 3, 500);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(104, 7, 80);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(104, 8, 60);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(105, 1, 666);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(105, 2, 66);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(105, 3, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(105, 7, 75);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(105, 8, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(106, 1, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(106, 2, 150);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(106, 3, 500);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(106, 7, 80);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(106, 8, 60);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(107, 1, 400);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(107, 2, 40);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(107, 3, 600);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(107, 7, 45);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(107, 8, 24);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(108, 1, 480);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(108, 2, 30);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(108, 3, 600);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(108, 7, 45);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(108, 8, 30);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(109, 1, 600);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(109, 2, 90);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(109, 3, 300);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(109, 7, 48);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(109, 8, 36);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(110, 1, 400);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(110, 2, 40);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(110, 3, 600);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(110, 7, 45);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(110, 8, 24);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(111, 1, 600);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(111, 2, 90);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(111, 3, 300);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(111, 7, 48);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(111, 8, 36);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(112, 1, 133);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(112, 2, 13);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(112, 3, 200);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(112, 7, 15);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(112, 8, 10);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(113, 1, 160);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(113, 2, 10);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(113, 3, 200);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(113, 7, 15);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(113, 8, 10);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(114, 1, 200);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(114, 2, 30);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(114, 3, 100);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(114, 7, 16);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(114, 8, 12);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(115, 1, 133);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(115, 2, 13);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(115, 3, 200);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(115, 7, 15);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(115, 8, 10);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(116, 1, 200);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(116, 2, 30);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(116, 3, 100);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(116, 7, 16);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(116, 8, 12);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(119, 1, 666);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(119, 2, 66);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(119, 3, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(119, 7, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(119, 8, 75);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(120, 1, 800);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(120, 2, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(120, 3, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(120, 7, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(120, 8, 75);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(121, 1, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(121, 2, 150);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(121, 3, 500);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(121, 7, 60);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(121, 8, 80);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(122, 1, 666);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(122, 2, 66);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(122, 3, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(122, 7, 50);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(122, 8, 75);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(123, 1, 1000);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(123, 2, 150);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(123, 3, 500);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(123, 7, 60);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(123, 8, 80);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 1, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 2, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 3, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 4, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 5, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 6, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 7, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 8, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 9, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 10, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 11, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 12, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 13, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 14, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 15, 999);
INSERT INTO `loe_item_stat` (`id`, `stat`, `value`) VALUES
	(124, 16, 999);
/*!40000 ALTER TABLE `loe_item_stat` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_loot
CREATE TABLE IF NOT EXISTS `loe_loot` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `item` int(11) NOT NULL DEFAULT '0',
  `min` int(11) NOT NULL DEFAULT '0',
  `max` int(11) NOT NULL DEFAULT '0',
  `chance` float NOT NULL DEFAULT '0',
  `condition` tinyint(3) unsigned NOT NULL,
  `cnd_data` int(11) NOT NULL DEFAULT '-1',
  PRIMARY KEY (`id`,`item`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_loot: ~3 rows (приблизительно)
/*!40000 ALTER TABLE `loe_loot` DISABLE KEYS */;
INSERT INTO `loe_loot` (`id`, `item`, `min`, `max`, `chance`, `condition`, `cnd_data`) VALUES
	(1, -1, 150, 350, -1, 0, -1);
INSERT INTO `loe_loot` (`id`, `item`, `min`, `max`, `chance`, `condition`, `cnd_data`) VALUES
	(1, 102, 1, 1, 0.01, 0, -1);
INSERT INTO `loe_loot` (`id`, `item`, `min`, `max`, `chance`, `condition`, `cnd_data`) VALUES
	(1, 123, 1, 1, 0.01, 0, -1);
/*!40000 ALTER TABLE `loe_loot` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_map
CREATE TABLE IF NOT EXISTS `loe_map` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(32) NOT NULL,
  `flags` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_map: ~22 rows (приблизительно)
/*!40000 ALTER TABLE `loe_map` DISABLE KEYS */;
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(1, 'Cantermore', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(2, 'Ponydale', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(3, 'Cloudopolis', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(4, 'Heartlands', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(5, 'Ponydale_SugarCaneCorner', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(6, 'Ponydale_GemMines', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(7, 'SweetAppleOrchards', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(8, 'Evershade_CastleGrounds', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(9, 'Evershade_Seclude', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(10, 'Evershade', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(11, 'Ponydale_Library', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(12, 'Ponydale_Library_2ndFloor', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(13, 'Ponydale_TheBoutique', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(14, 'CK', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(15, 'CK_Spa', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(16, 'CK_Stadium', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(17, 'CK_Castle', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(18, 'CK_Library', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(19, 'Appaloosa', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(20, 'Heartlands_Cottage', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(21, 'DevPlayground', 0);
INSERT INTO `loe_map` (`id`, `name`, `flags`) VALUES
	(22, 'DevPlayground_Castle', 0);
/*!40000 ALTER TABLE `loe_map` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_map_object
CREATE TABLE IF NOT EXISTS `loe_map_object` (
  `map` int(11) NOT NULL DEFAULT '0',
  `guid` smallint(5) unsigned NOT NULL DEFAULT '0',
  `object` int(10) NOT NULL DEFAULT '0',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `flags` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `pos_x` float NOT NULL DEFAULT '0',
  `pos_y` float NOT NULL DEFAULT '0',
  `pos_z` float NOT NULL DEFAULT '0',
  `rot_x` float NOT NULL DEFAULT '0',
  `rot_y` float NOT NULL DEFAULT '0',
  `rot_z` float NOT NULL DEFAULT '0',
  `time` float NOT NULL DEFAULT '0',
  `data01` int(11) NOT NULL DEFAULT '0',
  `data02` int(11) NOT NULL DEFAULT '0',
  `data03` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`map`,`guid`,`object`,`type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_map_object: ~86 rows (приблизительно)
/*!40000 ALTER TABLE `loe_map_object` DISABLE KEYS */;
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(1, 0, -1, 0, 0, -72.8009, 0.305817, 152.089, 0, 18.8273, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(1, 1, -1, 1, 0, -57.86, 9.72, -186.51, 0, -1.76447, 0, -1, 19, 0, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(1, 3, -1, 1, 0, -57.86, 9.72, -186.51, 0, -1.76447, 0, -1, 2, 3, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(1, 4, -1, 1, 0, 593.948, 124.772, -86.292, 0, -174.743, 0, -1, 3, 1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(1, 5, -1, 1, 0, -522.124, 15.2979, -395.044, 0, 12.0613, 0, -1, 4, 3, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(1, 6, -1, 1, 0, -57.86, 9.72, -186.51, 0, -1.76447, 0, -1, 14, 3, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 0, -1, 0, 0, 407.922, 125.152, 363.127, 0, 159.052, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 0, 1, 0, 0, 339.836, 125.82, 356.032, 0, -87.2254, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 0, 4, 0, 0, 354.903, 125.819, 352.864, 0, 0, 0, 0, 0, 0, 0);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 0, 5, 0, 0, 348.541, 125.819, 354.404, 0, 105.883, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 0, 71, 1, 1, 338.203, 125.92, 354.019, 0, 81.3118, 0, 5, 1, 6, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 1, -1, 1, 0, 527.15, 128.88, 256.512, 0, -90.9747, 0, -1, 19, 0, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 1, 4, 2, 1, 321.916, 125.82, 360.091, 0, 9.88238, 0, 15, 45, 50, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 1, 5, 2, 1, 309.288, 125.819, 368.377, 0, 2.0944, 0, 180, 49, 50, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 2, -1, 1, 0, 159.901, 149.085, 88.4331, 0, -36.5257, 0, -1, 6, 3, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 2, 4, 2, 1, 321.006, 125.816, 355.194, 0, -1.40448, 0, 15, 48, 49, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 3, -1, 1, 0, 527.15, 128.88, 256.512, 0, -90.9747, 0, -1, 1, 3, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 3, 4, 2, 1, 321.343, 125.816, 364.795, 0, 0.714559, 0, 15, 43, 47, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 4, 4, 2, 1, 326.159, 125.816, 358.747, 0, 2.24223, 0, 15, 50, 50, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 9, -1, 1, 0, 471.52, 128.498, 252.638, 0, 141.01, 0, -1, 11, 1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 10, -1, 1, 0, 550.258, 128.76, 242.966, 0, -86.9529, 0, -1, 7, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 20, -1, 1, 0, 312.169, 126.89, 342.991, 0, 12.0613, 0, -1, 5, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 21, -1, 1, 0, 356.2, 134.863, 192.19, 0, -26.6491, 0, -1, 4, 4, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 22, -1, 1, 0, 439.978, 121.302, 523.85, 0, -144.104, 0, -1, 10, 1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 23, -1, 1, 0, 92.8586, 127.791, 450.25, 0, 92.8343, 0, -1, 13, 1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(2, 24, -1, 1, 0, 527.15, 128.88, 256.512, 0, -90.9747, 0, -1, 14, 1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(3, 0, -1, 0, 0, -312.4, 66.1, 61.3, 0, -62.7836, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(3, 1, -1, 1, 0, -609.316, 193.066, -304.525, 0, -24.2635, 0, -1, 1, 4, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(3, 2, -1, 1, 0, -603.556, 193.066, -322.172, 0, -97.8467, 0, -1, 4, 1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(4, 0, -1, 0, 0, 3073.05, 182.394, 2875.16, 0, -135.319, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(4, 1, -1, 1, 0, 2433.14, 204.992, 3137.68, 0, 148.531, 0, -1, 3, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(4, 2, -1, 1, 0, 2870.94, 179.653, 2647.77, 0, 12.0613, 0, -1, 20, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(4, 3, -1, 1, 0, 3347.8, 194.254, 3316.62, 0, -124.558, 0, -1, 1, 5, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(4, 4, -1, 1, 0, 2527.34, 133.042, 1985.55, 0, 42.2593, 0, -1, 2, 21, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(5, 0, -1, 0, 0, 13.709, 0.530514, -2.78594, 0, -58.4786, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(5, 0, 2, 0, 0, -2.31505, -0.0602076, -4.60333, 0, -35.294, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(5, 0, 3, 0, 0, -3.7974, -0.0602076, -3.07965, 0, 120, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(5, 2, -1, 1, 0, 7.14576, 0.0332768, 4.90921, 0, -180, 0, -1, 2, 20, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(6, 0, -1, 0, 0, 45.6359, 9.42148, -61.0323, 0, 0, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(6, 3, -1, 1, 0, 45.4292, 9.44209, -65.8368, 0, -5.12, 0, -1, 2, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(7, 0, -1, 0, 0, 54.5055, -106.394, 19.2626, 0, 0, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(7, 2, -1, 1, 0, 60.51, -110.086, 60.8552, 0, -167.036, 0, -1, 2, 10, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(8, 0, -1, 0, 0, 623.618, 25.5159, -293.03, 0, -111.796, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(8, 1, -1, 1, 0, 626.284, 26.165, -292.638, 0, -114.379, 0, -1, 10, 3, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(9, 1, -1, 1, 0, -208.365, 0, -0.460869, 0, 57.5907, 0, -1, 10, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(10, 0, -1, 0, 0, 306.293, 17.3703, -450.612, 0, 0, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(10, 1, -1, 1, 0, 306.553, 17.3703, -451.984, 0, -8.47344, 0, -1, 2, 22, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(10, 2, -1, 1, 0, -324.417, 52.4534, -416.503, 0, -6.13475, 0, -1, 9, 1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(10, 3, -1, 1, 0, 440.692, 17.4075, 60.1733, 0, -51.7289, 0, -1, 8, 1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(11, 0, -1, 0, 0, -267.216, -199.428, 265.419, 0, 0, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(11, 1, -1, 1, 0, -265.13, -199.428, 265.022, 0, -76.4997, 0, -1, 2, 9, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(11, 2, -1, 1, 0, -287.123, -194.264, 260.638, 0, 119.067, 0, -1, 12, 1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(12, 0, -1, 0, 0, 4.37756, -178.505, 295.629, 0, 0, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(12, 1, -1, 1, 0, 3.58476, -178.505, 291.344, 0, 12.0613, 0, -1, 11, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(13, 0, -1, 0, 0, 26.5, -1, -57.5, 0, -155, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(13, 1, -1, 1, 0, 31.6424, -1.03606, -62.5336, 0, -44.0279, 0, -1, 2, 23, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(14, 0, -1, 0, 0, 8.66502, 70.0975, 379.862, 0, -153.066, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(14, 1, -1, 1, 0, -231.228, 41.0413, -26.2142, 0, 37.1243, 0, -1, 2, 24, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(14, 3, -1, 1, 0, -231.228, 41.0413, -26.2142, 0, 37.1243, 0, -1, 1, 6, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(14, 5, -1, 1, 0, -112.07, 71.746, 487.774, 0, 35.9571, 0, -1, 18, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(14, 6, -1, 1, 0, 15.9, 71.99, 352.863, 0, -16.5206, 0, -1, 17, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(14, 7, -1, 1, 0, 73.7767, 71.8067, 435.731, 0, 12.0613, 0, -1, 16, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(14, 8, -1, 1, 0, 168.3, 70.92, 370.63, 0, 12.0614, 0, -1, 15, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(15, 0, -1, 0, 0, -0.414463, 0, -8.4752, 0, 137.8, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(15, 2, -1, 1, 0, 0.151571, 0.54816, -11.2205, 0, 179.861, 0, -1, 14, 8, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(16, 0, -1, 0, 0, -0.0275289, 0.1659, -20.2115, 0, -89.9999, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(16, 2, -1, 1, 0, 0.0268059, 0.53684, -23.5386, 0, 0, 0, -1, 14, 7, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(16, 3, -1, 1, 0, 30.8026, 12.4931, 0.000785828, 0, -90, 0, -1, 17, 3, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(17, 0, -1, 0, 0, 0.00363281, 0.00500011, 0.0212498, 0, 0, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(17, 2, -1, 1, 0, 9.06963, 0.502, -1.65275, 0, -88.3385, 0, -1, 14, 6, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(17, 3, -1, 1, 0, -0.115367, 4.609, 20.7372, 0, -178.339, 0, -1, 16, 3, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(18, 0, -1, 0, 0, -0.406367, 1.035, 3.75125, 0, 0, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(18, 2, -1, 1, 0, -0.453039, 1.575, 1.08087, 0, 1.66141, 0, -1, 14, 5, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(19, 0, -1, 0, 0, -0.0650635, 0.0760002, -0.00601196, 0, 0, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(20, 0, -1, 0, 0, -1.149, 0.005, -0.565, 0, 89.9995, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(20, 2, -1, 1, 0, -4.361, 0.553149, 1.196, 0, 12.0613, 0, -1, 4, 2, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(21, 0, -1, 0, 0, 407.922, 125.152, 363.127, 0, 159.052, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(21, 21, -1, 1, 0, 395.974, 125.267, 374.598, 0, -26.6491, 0, -1, 3, 0, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(21, 22, -1, 1, 0, 386.964, 125.267, 378.439, 0, -26.6491, 0, -1, 1, 0, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(21, 23, -1, 1, 0, 380.364, 125.267, 375.728, 0, -26.6491, 0, -1, 2, 0, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(21, 24, -1, 1, 0, 396.534, 125.631, 366.978, 0, -26.6491, 0, -1, 14, 0, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(21, 25, -1, 1, 0, 377.663, 125.267, 369.244, 0, -26.6491, 0, -1, 4, 0, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(21, 28, -1, 1, 0, -114.16, 113.991, 973.1, 0, 93.205, 0, -1, 22, 20, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(21, 29, -1, 1, 0, 527.15, 128.88, 256.512, 0, -90.9747, 0, -1, 19, 0, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(22, 0, -1, 0, 0, 99.3614, 62.641, -205.404, 0, 146.991, 0, -1, -1, -1, -1);
INSERT INTO `loe_map_object` (`map`, `guid`, `object`, `type`, `flags`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`, `time`, `data01`, `data02`, `data03`) VALUES
	(22, 20, -1, 1, 0, 102.73, 61.85, -205.48, 0, 12.0614, 0, -1, 21, 28, -1);
/*!40000 ALTER TABLE `loe_map_object` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_message
CREATE TABLE IF NOT EXISTS `loe_message` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `emotion` smallint(5) unsigned NOT NULL DEFAULT '0',
  `message` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=50 DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_message: ~49 rows (приблизительно)
/*!40000 ALTER TABLE `loe_message` DISABLE KEYS */;
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(1, 0, 'Hello {name}! You awesome {race}!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(2, 0, 'The Answer to the Ultimate Question of Life, the Universe, and Everything?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(3, 0, '42x5');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(4, 0, '32x10');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(5, 0, '2Ax16');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(6, 0, '101000x2');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(7, 0, 'You wrong! Get away from me!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(8, 0, 'Get away from me!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(9, 0, 'Congratulations, you right!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(10, 0, 'No questions for you :(');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(11, 0, 'Listen, dude, i tell you something horrible! Yesterday, when i came to the forest for my herbs...');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(12, 0, 'Herbs? Why?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(13, 0, 'Oh, that... It\'s... for medical purpose...');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(14, 0, 'So, you\'re ill? Why don\'t you go to the doctor?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(15, 0, 'I... I can handle it by myself...');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(16, 0, 'But...');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(17, 0, 'Just listen! When i came to the forest and started collecting my herbs, suddenly giant wood wolf appeared and tried to eat me! And what worse, when I ran for my life, I lost a bag with my HERBS! What am I gonna do without them?!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(18, 0, 'Oh my goodness! Are you alright?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(19, 0, 'Of course not! I too scared to return for my bag... My herbs are lost forever!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(20, 0, 'But why can\'t you find this herbs in a different place?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(21, 0, 'It\'s very special! Grow only near ruins in Everfree Forest! What am gonna do?!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(22, 0, 'Maybe... Just maybe it\'s time to go to the doctor?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(23, 0, 'You\'re not helping...');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(24, 0, 'Excuse me, you said you lost something?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(25, 0, 'Oh, yes! It\'s a matter of life and death! I need my herbs ... to heal my disease... I can die without them! It\'s in big green bag somewhere near ruins in Everfree Forest. Can you find it?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(26, 0, 'Sure, I can\'t let you die! I\'ll do my best to find your bag!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(27, 0, 'Sorry, I don\'t have time for this.');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(28, 0, 'Oh, it\'s alright! Just one poor pony will die in unspeakable pain, nothing to worry about.');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(29, 0, 'I need my herbs ... to heal my disease... I can die without them! It\'s in big green bag somewhere near ruins in Everfree Forest. Can you find it?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(30, 0, 'You think she really will die without this herbs?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(31, 0, 'Thank you so much! You\'ll save my life!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(32, 0, 'Did you find the herbs?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(33, 0, 'Yes');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(34, 0, 'No');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(35, 0, 'Thank you so much for saving my life! Here some bits for your help!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(36, 0, 'Thank you so much for saving my life!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(37, 0, 'I hope she will be alright. Thanks for your help!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(38, 0, '(quiet) Psst. Hey you. Yes you {race}:l!{n}I have some special offers for you, wanna see?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(39, 0, '(quiet) Delicious bananas only for 10 bits!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(40, 0, 'Bananas... You kidding me! You kidding me, right?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(41, 0, 'Yes, I take a banana!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(42, 0, 'Yes, I take ten bananas!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(43, 0, 'No, I don\'t even like bananas...');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(44, 0, 'Quiet! You want SHE came here? SHE is always on guard, always listen, SHE take all my bananas and sends us to The Moon!');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(45, 0, 'If you change your mind, you know where to find me.');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(46, 0, 'Some more?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(47, 0, 'I don\'t want to trade with you now. Come back late.');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(48, 0, 'You back. Wanna see my offers?');
INSERT INTO `loe_message` (`id`, `emotion`, `message`) VALUES
	(49, 0, 'Ok, follow me.');
/*!40000 ALTER TABLE `loe_message` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_movement
CREATE TABLE IF NOT EXISTS `loe_movement` (
  `id` smallint(6) unsigned NOT NULL AUTO_INCREMENT,
  `state` smallint(6) unsigned NOT NULL DEFAULT '0',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `data01` int(10) NOT NULL DEFAULT '-1',
  `data02` int(10) NOT NULL DEFAULT '-1',
  `command` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `cmdData01` int(11) NOT NULL DEFAULT '-1',
  `cmdData02` int(11) NOT NULL DEFAULT '-1',
  `pos_x` float NOT NULL DEFAULT '0',
  `pos_y` float NOT NULL DEFAULT '0',
  `pos_z` float NOT NULL DEFAULT '0',
  `rot_x` float NOT NULL DEFAULT '0',
  `rot_y` float NOT NULL DEFAULT '0',
  `rot_z` float NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`,`state`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_movement: ~62 rows (приблизительно)
/*!40000 ALTER TABLE `loe_movement` DISABLE KEYS */;
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 0, 0, -1, -1, 1, 100, -1, 354.903, 125.819, 352.864, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 1, 0, -1, -1, 0, -1, -1, 350.273, 125.819, 357.024, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 2, 0, -1, -1, 0, -1, -1, 347.959, 125.82, 359.155, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 3, 0, -1, -1, 0, -1, -1, 345.648, 125.819, 361.283, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 4, 0, -1, -1, 0, -1, -1, 343.336, 125.819, 363.412, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 5, 0, -1, -1, 0, -1, -1, 341.01, 125.819, 365.529, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 6, 0, -1, -1, 0, -1, -1, 338.622, 125.82, 367.569, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 7, 0, -1, -1, 0, -1, -1, 336.097, 125.819, 369.44, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 8, 0, -1, -1, 0, -1, -1, 333.418, 125.819, 371.082, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 9, 0, -1, -1, 0, -1, -1, 330.674, 125.819, 372.615, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 10, 0, -1, -1, 0, -1, -1, 327.858, 125.819, 374.011, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 11, 0, -1, -1, 0, -1, -1, 324.986, 125.819, 375.288, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 12, 0, -1, -1, 0, -1, -1, 321.993, 125.819, 376.245, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 13, 0, -1, -1, 0, -1, -1, 318.946, 125.819, 377.014, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 14, 0, -1, -1, 1, 350, -1, 315.839, 125.819, 377.489, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 15, 0, -1, -1, 0, -1, -1, 312.707, 125.819, 377.72, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 16, 0, -1, -1, 0, -1, -1, 309.565, 125.811, 377.681, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 17, 0, -1, -1, 0, -1, -1, 306.466, 125.819, 377.198, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 18, 0, -1, -1, 0, -1, -1, 303.445, 125.819, 376.329, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 19, 0, -1, -1, 0, -1, -1, 300.446, 125.819, 375.388, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 20, 0, -1, -1, 0, -1, -1, 297.548, 125.82, 374.202, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 21, 0, -1, -1, 0, -1, -1, 295.165, 125.82, 372.169, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 22, 0, -1, -1, 0, -1, -1, 293.446, 125.827, 369.561, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 23, 0, -1, -1, 0, -1, -1, 292.948, 125.881, 366.475, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 24, 0, -1, -1, 0, -1, -1, 293.889, 125.823, 363.498, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 25, 0, -1, -1, 0, -1, -1, 295.471, 125.819, 360.787, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 26, 0, -1, -1, 0, -1, -1, 297.142, 125.84, 358.126, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 27, 0, -1, -1, 0, -1, -1, 299.078, 125.865, 355.657, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 28, 0, -1, -1, 0, -1, -1, 301.402, 125.886, 353.547, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 29, 0, -1, -1, 0, -1, -1, 303.94, 125.866, 351.692, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 30, 1, 6, 10, 0, -1, -1, 306.521, 125.849, 349.9, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 31, 0, -1, -1, 0, -1, -1, 309.281, 125.838, 348.398, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 32, 0, -1, -1, 0, -1, -1, 312.172, 125.82, 347.165, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 33, 0, -1, -1, 0, -1, -1, 315.146, 125.819, 346.151, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 34, 0, -1, -1, 0, -1, -1, 318.213, 125.819, 345.467, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 35, 0, -1, -1, 0, -1, -1, 321.326, 125.819, 345.022, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 36, 0, -1, -1, 0, -1, -1, 324.456, 125.819, 344.767, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 37, 0, -1, -1, 0, -1, -1, 327.599, 125.819, 344.716, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 38, 0, -1, -1, 0, -1, -1, 330.738, 125.819, 344.859, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 39, 0, -1, -1, 0, -1, -1, 333.868, 125.819, 345.143, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 40, 0, -1, -1, 0, -1, -1, 336.976, 125.819, 345.612, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 41, 0, -1, -1, 0, -1, -1, 340.065, 125.819, 346.19, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 42, 0, -1, -1, 0, -1, -1, 343.116, 125.819, 346.944, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 43, 0, -1, -1, 0, -1, -1, 346.111, 125.819, 347.891, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 44, 0, -1, -1, 0, -1, -1, 349.008, 125.819, 349.108, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(1, 45, 0, -1, -1, 5, 0, -1, 351.592, 125.819, 350.653, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 0, 1, -1, -1, 1, 125, -1, 348.541, 125.819, 354.404, 0, 105.883, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 1, 0, -1, -1, 0, -1, -1, 349.669, 125.819, 354.88, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 2, 0, -1, -1, 0, -1, -1, 351.913, 125.819, 357.094, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 3, 0, -1, -1, 0, -1, -1, 354.146, 125.819, 359.306, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 4, 0, -1, -1, 0, -1, -1, 356.315, 125.746, 361.584, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 5, 0, -1, -1, 0, -1, -1, 358.316, 125.418, 364, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 6, 0, -1, -1, 0, -1, -1, 359.775, 125.273, 366.779, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 7, 0, -1, -1, 0, -1, -1, 360.908, 125.196, 369.71, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 8, 0, -1, -1, 0, -1, -1, 361.843, 125.36, 372.711, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 9, 0, -1, -1, 0, -1, -1, 362.759, 125.908, 375.718, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 10, 0, -1, -1, 0, -1, -1, 363.672, 126.312, 378.727, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 11, 0, -1, -1, 0, -1, -1, 364.581, 126.329, 381.733, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 12, 0, -1, -1, 0, -1, -1, 365.024, 125.763, 384.842, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 13, 0, -1, -1, 0, -1, -1, 365.167, 124.95, 387.772, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 14, 0, -1, -1, 0, -1, -1, 365.566, 124.702, 390.883, 0, 0, 0);
INSERT INTO `loe_movement` (`id`, `state`, `type`, `data01`, `data02`, `command`, `cmdData01`, `cmdData02`, `pos_x`, `pos_y`, `pos_z`, `rot_x`, `rot_y`, `rot_z`) VALUES
	(2, 15, 1, -1, -1, 0, -1, -1, 366.975, 124.702, 393.561, 0, -142.666, 0);
/*!40000 ALTER TABLE `loe_movement` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_npc
CREATE TABLE IF NOT EXISTS `loe_npc` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `flags` tinyint(3) unsigned NOT NULL,
  `level` smallint(6) NOT NULL,
  `dialog` smallint(5) unsigned NOT NULL,
  `index` tinyint(3) unsigned NOT NULL,
  `movement` smallint(5) unsigned NOT NULL,
  `name` varchar(64) NOT NULL,
  `race` tinyint(3) unsigned NOT NULL,
  `gender` tinyint(3) unsigned NOT NULL,
  `eye` smallint(6) NOT NULL,
  `tail` smallint(6) NOT NULL,
  `hoof` smallint(6) NOT NULL,
  `mane` smallint(6) NOT NULL,
  `bodysize` float NOT NULL,
  `hornsize` float NOT NULL,
  `eyecolor` int(11) NOT NULL,
  `hoofcolor` int(11) NOT NULL,
  `bodycolor` int(11) NOT NULL,
  `haircolor0` int(11) NOT NULL,
  `haircolor1` int(11) NOT NULL,
  `haircolor2` int(11) NOT NULL,
  `cutiemark0` int(11) NOT NULL,
  `cutiemark1` int(11) NOT NULL,
  `cutiemark2` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_npc: ~4 rows (приблизительно)
/*!40000 ALTER TABLE `loe_npc` DISABLE KEYS */;
INSERT INTO `loe_npc` (`id`, `flags`, `level`, `dialog`, `index`, `movement`, `name`, `race`, `gender`, `eye`, `tail`, `hoof`, `mane`, `bodysize`, `hornsize`, `eyecolor`, `hoofcolor`, `bodycolor`, `haircolor0`, `haircolor1`, `haircolor2`, `cutiemark0`, `cutiemark1`, `cutiemark2`) VALUES
	(1, 7, 5, 1, 0, 0, 'Frederic Dash', 2, 2, 1, 10, 0, 16, 0.9, 0.98497, 8322980, 0, 16767615, 16744172, 16752626, 16744172, 54, 0, 0);
INSERT INTO `loe_npc` (`id`, `flags`, `level`, `dialog`, `index`, `movement`, `name`, `race`, `gender`, `eye`, `tail`, `hoof`, `mane`, `bodysize`, `hornsize`, `eyecolor`, `hoofcolor`, `bodycolor`, `haircolor0`, `haircolor1`, `haircolor2`, `cutiemark0`, `cutiemark1`, `cutiemark2`) VALUES
	(2, 4, 4, 2, 0, 0, 'Datura  Herb', 1, 2, 0, 13, 0, 13, 1.015, 1.06999, 3735528, 0, 8388489, 5414502, 718335, 15397375, 102, 0, 0);
INSERT INTO `loe_npc` (`id`, `flags`, `level`, `dialog`, `index`, `movement`, `name`, `race`, `gender`, `eye`, `tail`, `hoof`, `mane`, `bodysize`, `hornsize`, `eyecolor`, `hoofcolor`, `bodycolor`, `haircolor0`, `haircolor1`, `haircolor2`, `cutiemark0`, `cutiemark1`, `cutiemark2`) VALUES
	(3, 4, 6, 2, 1, 0, 'Tropical Tail', 3, 3, 1, 3, 0, 11, 1.045, 1.19997, 16764265, 0, 7533055, 5880319, 16775150, 16772854, 88, 0, 0);
INSERT INTO `loe_npc` (`id`, `flags`, `level`, `dialog`, `index`, `movement`, `name`, `race`, `gender`, `eye`, `tail`, `hoof`, `mane`, `bodysize`, `hornsize`, `eyecolor`, `hoofcolor`, `bodycolor`, `haircolor0`, `haircolor1`, `haircolor2`, `cutiemark0`, `cutiemark1`, `cutiemark2`) VALUES
	(4, 9, 25, 3, 0, 1, 'Jet Tarty', 1, 3, 0, 12, 0, 16, 1.05, 0.855, 8365055, 0, 13500287, 16762239, 16766111, 16748159, 238, 0, 0);
INSERT INTO `loe_npc` (`id`, `flags`, `level`, `dialog`, `index`, `movement`, `name`, `race`, `gender`, `eye`, `tail`, `hoof`, `mane`, `bodysize`, `hornsize`, `eyecolor`, `hoofcolor`, `bodycolor`, `haircolor0`, `haircolor1`, `haircolor2`, `cutiemark0`, `cutiemark1`, `cutiemark2`) VALUES
	(5, 12, 6, 3, 0, 2, 'Lucky Chance', 3, 3, 1, 13, 0, 11, 0.945, 0.945, 7339949, 0, 3531263, 4905759, 9633627, 16318326, 97, 0, 0);
/*!40000 ALTER TABLE `loe_npc` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_npc_trade
CREATE TABLE IF NOT EXISTS `loe_npc_trade` (
  `id` int(11) NOT NULL,
  `item` int(11) NOT NULL,
  PRIMARY KEY (`id`,`item`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_npc_trade: ~9 rows (приблизительно)
/*!40000 ALTER TABLE `loe_npc_trade` DISABLE KEYS */;
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(1, 102);
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(1, 103);
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(1, 104);
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(1, 105);
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(1, 106);
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(4, 54);
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(4, 55);
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(4, 56);
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(4, 57);
INSERT INTO `loe_npc_trade` (`id`, `item`) VALUES
	(4, 67);
/*!40000 ALTER TABLE `loe_npc_trade` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_npc_wear
CREATE TABLE IF NOT EXISTS `loe_npc_wear` (
  `id` int(11) NOT NULL,
  `slot01` int(11) NOT NULL,
  `slot02` int(11) NOT NULL,
  `slot03` int(11) NOT NULL,
  `slot04` int(11) NOT NULL,
  `slot05` int(11) NOT NULL,
  `slot06` int(11) NOT NULL,
  `slot07` int(11) NOT NULL,
  `slot08` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_npc_wear: ~0 rows (приблизительно)
/*!40000 ALTER TABLE `loe_npc_wear` DISABLE KEYS */;
INSERT INTO `loe_npc_wear` (`id`, `slot01`, `slot02`, `slot03`, `slot04`, `slot05`, `slot06`, `slot07`, `slot08`) VALUES
	(1, 134, 135, 20, -1, -1, -1, -1, -1);
/*!40000 ALTER TABLE `loe_npc_wear` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_resource
CREATE TABLE IF NOT EXISTS `loe_resource` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `path` varchar(64) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_resource: ~33 rows (приблизительно)
/*!40000 ALTER TABLE `loe_resource` DISABLE KEYS */;
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(0, 'playerbase');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(1, 'triggerpoint');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(2, 'picks/rock');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(3, 'picks/chest');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(4, 'picks/flowers');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(5, 'picks/mushroom');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(6, 'picks/randomgem');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(7, 'picks/dandelion');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(8, 'pets/dog');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(9, 'pets/bunny');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(10, 'pets/kittycat');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(11, 'mobs/bunny');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(12, 'mobs/dragon');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(13, 'mobs/hornet');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(14, 'mobs/cockatrice');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(15, 'mobs/timberwolf');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(16, 'mobs/birch dryad\n');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(17, 'mobs/manticore_v2.2');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(18, 'mobs/lantern monster\n');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(19, 'mobs/husky diamond dog');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(20, 'effects/gale');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(21, 'effects/apple');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(22, 'effects/aoe heal');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(23, 'effects/teleport');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(24, 'effects/fire burst');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(25, 'effects/dragonfire2');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(26, 'effects/healability');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(27, 'effects/poppyflowers');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(28, 'effects/rough terrain');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(29, 'effects/cockatricestare');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(30, 'effects/endteleportsparks');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(31, 'effects/groundrockhit gray1');
INSERT INTO `loe_resource` (`id`, `path`) VALUES
	(32, 'effects/finishteleportsparks');
/*!40000 ALTER TABLE `loe_resource` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_spell
CREATE TABLE IF NOT EXISTS `loe_spell` (
  `id` smallint(6) NOT NULL AUTO_INCREMENT,
  `index` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `effect` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `targets` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `data01` float NOT NULL DEFAULT '0',
  `data02` float NOT NULL DEFAULT '0',
  `data03` float NOT NULL DEFAULT '0',
  `base_const` float NOT NULL DEFAULT '0',
  `mod_level` float NOT NULL DEFAULT '0',
  `mod_attack` float NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`,`index`,`effect`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_spell: ~16 rows (приблизительно)
/*!40000 ALTER TABLE `loe_spell` DISABLE KEYS */;
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(2, 0, 255, 7, 80, 32, 2.5, 0, 0, 0);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(5, 0, 1, 2, 60, 2.5, 3, 40, 25, 2);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(9, 0, 250, 15, 80, 32, 12, 6, 6, 8);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(9, 1, 254, 2, 0, 0, 0, 5, 6.5, 0.55);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(9, 2, 253, 4, 0, 0, 0, 2.5, 5.5, 0.45);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(10, 0, 0, 15, 40, 2.5, 1.5, 0, 0, 0);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(10, 1, 1, 2, 0, 0, 0, 25, 11.5, 1.15);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(10, 2, 2, 130, 0, 6, 0, 10, 5, 0.85);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(11, 0, 1, 2, 40, 32, 1.5, 15, 14.5, 4.5);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(12, 0, 10, 4, 50, 10, 3, 10, 4.5, 1.35);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(14, 0, 0, 15, 60, 32, 1.5, 0, 0, 0);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(14, 1, 3, 2, 3, 5.5, 0, 25, 12, 1.15);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(15, 0, 5, 2, 50, 28, 2, 35, 15, 2.5);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(16, 0, 250, 15, 80, 32, 18, 9, 14, 16);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(16, 1, 252, 2, 0, 0, 0, 6, 7.5, 0.75);
INSERT INTO `loe_spell` (`id`, `index`, `effect`, `targets`, `data01`, `data02`, `data03`, `base_const`, `mod_level`, `mod_attack`) VALUES
	(21, 0, 1, 2, 50, 16, 2.5, 25, 25, 1.5);
/*!40000 ALTER TABLE `loe_spell` ENABLE KEYS */;


-- Дамп структуры для таблица legends_of_equestria.loe_user
CREATE TABLE IF NOT EXISTS `loe_user` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `login` char(32) NOT NULL,
  `phash` char(40) NOT NULL,
  `access` tinyint(4) unsigned NOT NULL DEFAULT '0',
  `session` char(64) DEFAULT NULL,
  `data` blob,
  PRIMARY KEY (`id`),
  UNIQUE KEY `login` (`login`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Дамп данных таблицы legends_of_equestria.loe_user: ~3 rows (приблизительно)
/*!40000 ALTER TABLE `loe_user` DISABLE KEYS */;
/*!40000 ALTER TABLE `loe_user` ENABLE KEYS */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
