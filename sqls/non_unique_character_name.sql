ALTER TABLE `loe_character` DROP INDEX `CHARACTER`, ADD UNIQUE INDEX `CHARACTER` (`race`, `name`, `vdata`(100), `gender`);