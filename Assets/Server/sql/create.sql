/* Drop existing tables */
DROP TABLE IF EXISTS DAMAGE_TYPE;
DROP TABLE IF EXISTS ORIGIN_CELL;
DROP TABLE IF EXISTS ACTION_TYPE;
DROP TABLE IF EXISTS EFFECT_TYPE;
DROP TABLE IF EXISTS ITEM_TYPE;
DROP TABLE IF EXISTS WEAPON_TYPE;
DROP TABLE IF EXISTS SKILL_TYPE;
DROP TABLE IF EXISTS ATTRIBUTE_TYPE;
DROP TABLE IF EXISTS RESOURCE_TYPE;
DROP TABLE IF EXISTS EQUIPMENT_SLOT;

DROP TABLE IF EXISTS Ability;
DROP TABLE IF EXISTS Ability_Action;
DROP TABLE IF EXISTS Delay_Action;
DROP TABLE IF EXISTS Move_Action;
DROP TABLE IF EXISTS Spawn_Action;
DROP TABLE IF EXISTS Projectile;
DROP TABLE IF EXISTS Cell;
DROP TABLE IF EXISTS Effect;
DROP TABLE IF EXISTS Damage_Source;
DROP TABLE IF EXISTS Item;
DROP TABLE IF EXISTS Armor;
DROP TABLE IF EXISTS Weapon;
DROP TABLE IF EXISTS Actions;
DROP TABLE IF EXISTS Spawn_Cells;
DROP TABLE IF EXISTS Projectile_Effects;
DROP TABLE IF EXISTS Projectile_Damage_Sources;
DROP TABLE IF EXISTS Weapon_Damage;
DROP TABLE IF EXISTS User;
DROP TABLE IF EXISTS Attribute;
DROP TABLE IF EXISTS Skill;
DROP TABLE IF EXISTS Resource;
DROP TABLE IF EXISTS Equipment;
DROP TABLE IF EXISTS Inventory;
DROP TABLE IF EXISTS Item_Slot;
DROP TABLE IF EXISTS Location;
DROP TABLE IF EXISTS Player_Character;
DROP TABLE IF EXISTS Active_Abilities;
DROP TABLE IF EXISTS Learned_Abilities;
DROP TABLE IF EXISTS Friend;
DROP TABLE IF EXISTS Message;
DROP TABLE IF EXISTS Message_Participant;
DROP TABLE IF EXISTS Conversation;
DROP TABLE IF EXISTS Conversation_Participant;
DROP TABLE IF EXISTS Direct_Message;
DROP TABLE IF EXISTS Location_Destination;
DROP TABLE IF EXISTS Discovered_Location;
DROP TABLE IF EXISTS Route;
DROP TABLE IF EXISTS Location_Scene;
DROP TABLE IF EXISTS Item_Modifiers;


/****
ENUMS
****/
CREATE TABLE DAMAGE_TYPE (
	[damage_type_id] 	INTEGER PRIMARY KEY,
	[damage_type_name]	VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO DAMAGE_TYPE (damage_type_id, damage_type_name)
VALUES
	(0, "ARCANE"),
	(1, "BLOOD"),
	(2, "BLUDGEONING"),
	(3, "FIRE"),
	(4, "LIGHT"),
	(5, "LIGHTNING"),
	(6, "MENTAL"),
	(7, "PIERCING"),
	(8, "PRIMAL"),
	(9, "SLASHING"),
	(10, "SPIRIT"),
	(11, "VOID");


CREATE TABLE ORIGIN_CELL (
	[origin_cell_id]	INTEGER PRIMARY KEY,
	[cell_name]			VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO ORIGIN_CELL (origin_cell_id, cell_name)
VALUES
	(0, "RELATIVE"),
	(1, "ABSOLUTE"),
	(2, "NEAREST_ENEMY");


CREATE TABLE ACTION_TYPE (
	[action_type_id]	INTEGER PRIMARY KEY,
	[action_type_name]	VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO ACTION_TYPE (action_type_id, action_type_name)
VALUES
	(0, "MOVE"),
	(1, "DELAY"),
	(2, "SPAWN");


CREATE TABLE EFFECT_TYPE (
	[effect_type_id]	INTEGER PRIMARY KEY,
	[effect_type_name]	VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO EFFECT_TYPE (effect_type_id, effect_type_name)
VALUES
	(0, "AFFECT HEALTH"),
	(1, "AFFECT MANA"),
	(2, "AFFECT ATTRIBUTE"),
	(3, "AFFECT SKILL"),
	(4, "AFFECT DAMAGE"),
	(5, "AFFECT RESISTANCE");


CREATE TABLE ITEM_TYPE (
	[item_type_id] 		INTEGER PRIMARY KEY,
	[item_type_name] 	VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO ITEM_TYPE (item_type_id, item_type_name)
VALUES
	(0, "CONSUMABLE"),
	(1, "WEAPON"),
	(2, "ARMOR");


CREATE TABLE WEAPON_TYPE (
	[weapon_type_id]	INTEGER PRIMARY KEY,
	[weapon_type_name]	VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO WEAPON_TYPE (weapon_type_id, weapon_type_name)
VALUES
	(0, "AXE"),
	(1, "BOW"),
	(2, "MACE"),
	(3, "POLEARM"),
	(4, "QUARTERSTAFF"),
	(5, "STAFF"),
	(6, "SWORD");


CREATE TABLE SKILL_TYPE (
	[skill_type_id]		INTEGER PRIMARY KEY,
	[skill_type_name]	VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO SKILL_TYPE (skill_type_id, skill_type_name)
VALUES
	(0, "POLEARMS"),
	(1, "SWORDS"),
	(2, "AXES"),
	(3, "BLUNT"),
	(4, "BOW"),
	(5, "LIGHT_ARMOR"),
	(6, "HEAVY_ARMOR"),
	(7, "SHIELDS"),
	(8, "ELEMENTAL"),
	(9, "ARCANE"),
	(10, "ESSENCE");


CREATE TABLE ATTRIBUTE_TYPE (
	[attribute_type_id]		INTEGER PRIMARY KEY,
	[attribute_type_name]	VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO ATTRIBUTE_TYPE (attribute_type_id, attribute_type_name)
VALUES
	(0, "VITALITY"),
	(1, "AGILITY"),
	(2, "AFFINITY");


CREATE TABLE RESOURCE_TYPE (
	[resource_type_id]		INTEGER PRIMARY KEY,
	[resource_type_name]	VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO RESOURCE_TYPE (resource_type_id, resource_type_name)
VALUES
	(0, "HEALTH"),
	(1, "MANA"),
	(2, "STAMINA"),
	(3, "ENERGY");


CREATE TABLE EQUIPMENT_SLOT (
	[equipment_slot_id]		INTEGER NOT NULL PRIMARY KEY,
	[equipment_slot_name]	VARCHAR(32) NOT NULL UNIQUE
);

INSERT INTO EQUIPMENT_SLOT (equipment_slot_id, equipment_slot_name)
VALUES
	(0, "HEAD"),
	(1, "TORSO"),
	(2, "ARMS"),
	(3, "LEGS"),
	(4, "MAIN_HAND"),
	(5, "OFF_HAND"),
	(6, "TRINKET");


/*****
TABLES
*****/
CREATE TABLE Ability (
	[ability_id] 		INTEGER PRIMARY KEY,
	[ability_name] 		VARCHAR(32) DEFAULT "NO NAME",
	[description] 		TEXT 		DEFAULT "NO DESCRIPTION",
	[casting_time] 		REAL 		DEFAULT 0,
	[cooldown] 			REAL 		DEFAULT 0,
	[mana_cost] 		INTEGER 		DEFAULT 0,
	[energy_cost] 		INTEGER 		DEFAULT 0,
	[stamina_cost] 		INTEGER 		DEFAULT 0,
	[can_move_cast] 	BOOLEAN 	DEFAULT 0,
	[can_move_activate] BOOLEAN 	DEFAULT 1
);

CREATE TABLE Ability_Action (
	[ability_action_id] INTEGER PRIMARY KEY,
	[action_type] 		INTEGER NOT NULL REFERENCES ACTION_TYPE(action_type_id),
	[use_cache] 		BOOLEAN DEFAULT 0
);

CREATE TABLE Delay_Action (
	[delay_action_id] 	INTEGER PRIMARY KEY,
	[duration] 			REAL DEFAULT 0
);

CREATE TABLE Move_Action (
	[move_action_id] 	INTEGER PRIMARY KEY,
	[origin_cell] 		INTEGER NOT NULL REFERENCES ORIGIN_CELL(origin_cell_id),
	[destination_x] 	INTEGER 	DEFAULT 0,
	[destination_y] 	INTEGER 	DEFAULT 0
);

CREATE TABLE Projectile (
	[projectile_id]	INTEGER PRIMARY KEY,
	[speed] 		REAL 	DEFAULT 0,
	[timeout]		REAL	DEFAULT 1,
	[range]			INTEGER		DEFAULT 1,
	[pierce]		BOOLEAN DEFAULT 0,
	[direction_x]	INTEGER		DEFAULT 1,
	[direction_y]	INTEGER		DEFAULT 0
);

CREATE TABLE Spawn_Action (
	[spawn_action_id] 	INTEGER PRIMARY KEY,
	[projectile_id]		INTEGER NOT NULL REFERENCES Projectile(projectile_id),
	[origin_cell]		INTEGER NOT NULL REFERENCES ORIGIN_CELL(origin_cell_id)
);

CREATE TABLE Cell (
	[cell_id] INTEGER PRIMARY KEY,
	[x_coord] INTEGER DEFAULT 0,
	[y_coord] INTEGER DEFAULT 0
);

CREATE TABLE Effect (
	[effect_id]		INTEGER PRIMARY KEY,
	[effect_type]	INTEGER NOT NULL REFERENCES EFFECT_TYPE(effect_type_id),
	[duration]		REAL 	DEFAULT 0,
	[amount]		REAL	DEFAULT 0,
	[specifier]		INTEGER	NOT NULL
);

CREATE TABLE Damage_Source (
	[damage_source_id] 	INTEGER PRIMARY KEY,
	[damage_type]		INTEGER NOT NULL REFERENCES DAMAGE_TYPE(damage_type_id),
	[amount]			INTEGER DEFAULT 1
);

CREATE TABLE Item (
	[item_id]		INTEGER 	PRIMARY KEY,
	[equip_slot]	INTEGER 	NOT NULL REFERENCES EQUIPMENT_SLOT(equipment_slot_id),
	[name]			VARCHAR(64)	NOT NULL,
	[description]	TEXT,
	[bulk]			REAL,
	[value]			INTEGER
);

CREATE TABLE Item_Modifiers (
	[item]		INTEGER NOT NULL REFERENCES Item(item_id),
	[attribute]	INTEGER NOT NULL REFERENCES ATTRIBUTE_TYPE(attribute_type_id),
	[amount]	INTEGER DEFAULT 0,
	PRIMARY KEY (item, attribute)
);

CREATE TABLE Inventory (
	[inventory_id] 	INTEGER PRIMARY KEY,
	[max_bulk]		REAL DEFAULT 10
);

CREATE TABLE Item_Slot (
	[item_id]		INTEGER NOT NULL REFERENCES Item(item_id),
	[inventory_id]	INTEGER NOT NULL REFERENCES Inventory(inventory_id),
	[amount]		INTEGER DEFAULT 0,
	PRIMARY KEY (item_id, inventory_id)
);

CREATE TABLE User (
	user_id		INTEGER		PRIMARY KEY,
	user_name	VARCHAR(32) UNIQUE NOT NULL,
	password	VARCHAR(32) NOT NULL
);

CREATE TABLE Attribute (
	[character]			INTEGER NOT NULL REFERENCES Player_Character(character_id),
	[attribute_type] 	INTEGER NOT NULL REFERENCES ATTRIBUTE_TYPE(attribute_type_id),
	[base_value]		INTEGER DEFAULT 5,
	[current_value]		INTEGER DEFAULT 5,
	PRIMARY KEY (character, attribute_type)
);

CREATE TABLE Skill (
	[character]		INTEGER NOT NULL REFERENCES Player_Character(character_id),
	[skill_type]	INTEGER NOT NULL REFERENCES SKILL_TYPE(skill_type_id),
	[base_value]	INTEGER DEFAULT 0,
	[current_value] INTEGER DEFAULT 0,
	[xp]			FLOAT 	DEFAULT 0,
	PRIMARY KEY (character, skill_type)
);

CREATE TABLE Resource (
	[character]		INTEGER NOT NULL REFERENCES Player_Character(character_id),
	[resource_type]	INTEGER NOT NULL REFERENCES RESOURCE_TYPE(resource_type_id),
	[max]			REAL 	DEFAULT 100,
	[min]			REAL 	DEFAULT 0,
	[current_value]	REAL 	DEFAULT 100,
	[regen_rate]	REAL 	DEFAULT 1,
	[regen]			BOOLEAN DEFAULT 0,
	PRIMARY KEY (character, resource_type)
);

CREATE TABLE Location (
	[location_id]	INTEGER 	PRIMARY KEY,
	[name]			VARCHAR(64)	NOT NULL,
	[description]	TEXT,
	[hidden]		BOOLEAN 	DEFAULT 0
);

CREATE TABLE Route (
	[route_id]		INTEGER 	PRIMARY KEY,
	[name]			VARCHAR(64)	NOT NULL,
	[description]	TEXT,
	[hidden]		BOOLEAN		DEFAULT 0,
	[length]		INTEGER		DEFAULT 1,
	[complexity]	INTEGER		DEFAULT 1,
	[danger]		INTEGER		DEFAULT 1
);

CREATE TABLE Location_Destination (
	[location]		INTEGER NOT NULL REFERENCES Location(location_id),
	[route]			INTEGER NOT NULL REFERENCES Route(route_id),
	[destination]	INTEGER NOT NULL REFERENCES Location(location_id),
	PRIMARY KEY (location, route, destination)
);

CREATE TABLE Location_Scene (
	[location]		INTEGER NOT NULL REFERENCES Location(location_id),
	[scene_name]	TEXT	NOT NULL,
	[scene_data]	TEXT,
	PRIMARY KEY (location, scene_name)
);

CREATE TABLE Player_Character (
	[character_id]	INTEGER 	PRIMARY KEY,
	[user_id]		INTEGER		NOT NULL REFERENCES User(user_id),
	[inventory_id]	INTEGER		NOT NULL REFERENCES Inventory(inventory_id),
	[location]		INTEGER		NOT NULL REFERENCES Location(location_id),
	[level]			INTEGER		DEFAULT 1,
	[name]			VARCHAR(32)	NOT NULL
);

CREATE TABLE Character_Equipment (
	[character]		INTEGER	NOT NULL REFERENCES Player_Character(character_id),
	[item]			INTEGER NOT NULL REFERENCES Item(item_id),
	[equip_slot]	INTEGER NOT NULL REFERENCES EQUIPMENT_SLOT(equipment_slot_id),
	PRIMARY KEY (character, equip_slot)
);

CREATE TABLE Discovered_Location (
	[location] 	INTEGER	NOT NULL REFERENCES Location(location_id),
	[character] INTEGER NOT NULL REFERENCES Player_Character(character_id),
	PRIMARY KEY (location, character)
);

CREATE TABLE Active_Abilities (
	[character_id]	INTEGER NOT NULL REFERENCES Player_Character(character_id),
	[ability_id]	INTEGER NOT NULL REFERENCES Ability(ability_id),
	[slot]			INTEGER NOT NULL
);


/********
RELATIONS
********/
CREATE TABLE Actions (
	[ability_action_id] INTEGER NOT NULL REFERENCES Ability_Action(ability_action_id),
	[ability_id] 		INTEGER NOT NULL REFERENCES Ability(ability_id)
);

CREATE TABLE Spawn_Cells (
	[spawn_action_id] 	INTEGER NOT NULL REFERENCES Spawn_Action(spawn_action_id),
	[cell_id]			INTEGER NOT NULL REFERENCES Spawn_Cells(cell_id)
);

CREATE TABLE Projectile_Effects (
	[projectile_id] INTEGER NOT NULL REFERENCES Projectile(projectile_id),
	[effect_id] 	INTEGER NOT NULL REFERENCES Effect(effect_id)
);

CREATE TABLE Projectile_Damage_Sources (
	[projectile_id]		INTEGER NOT NULL REFERENCES Projectile(projectile_id),
	[damage_source_id] 	INTEGER NOT NULL REFERENCES Damage_Source(damage_source_id)
);

CREATE TABLE Weapon_Damage (
	[weapon_id]			INTEGER NOT NULL REFERENCES Weapon(item_id),
	[damage_source_id]	INTEGER NOT NULL REFERENCES Damage_Source(damage_source_id)
);

CREATE TABLE Learned_Abilities (
	[character_id] 	INTEGER NOT NULL REFERENCES Player_Character(character_id),
	[ability_id]	INTEGER NOT NULL REFERENCES Ability(ability_id)
);

CREATE TABLE Friend (
	[user1]	INTEGER NOT NULL REFERENCES User(user_id),
	[user2]	INTEGER NOT NULL REFERENCES User(user_id),
	PRIMARY KEY (user1, user2)
);

CREATE TABLE Conversation (
	[conversation_id]	INTEGER	PRIMARY KEY
);

CREATE TABLE Conversation_Participant (
	[conversation]	INTEGER	NOT NULL REFERENCES Conversation(conversation_id),
	[user]			INTEGER NOT NULL REFERENCES User(user_id)
);

CREATE TABLE Message (
	[message_id]		INTEGER			IDENTITY,
	[conversation]		INTEGER			NOT NULL REFERENCES Conversation(conversation_id),
	[sender]			INTEGER			NOT NULL REFERENCES User(user_id),
	[time_sent]		TEXT			NOT NULL,
	[content]			VARCHAR(256)	NOT NULL,
	PRIMARY KEY (message_id, conversation)
);

CREATE TABLE Direct_Message (
	[message_id]		INTEGER			PRIMARY KEY,
	[sender]			INTEGER			NOT NULL REFERENCES User(user_id),
	[recipient]			INTEGER			NOT NULL REFERENCES User(user_id),
	[content]			VARCHAR(256)	NOT NULL,
	[time_sent]			TEXT			NOT NULL
);

/* All messages that are part of a given conversation */
/*SELECT Message.content
FROM Message
WHERE Message.conversation_id = given_id*/

/*All messages that are part of a given conversation and the user that sent them*/
/*SELECT Message.content, User.username
FROM Message JOIN User
WHERE Message.sender = User.user_id
	AND Message.conversation_id = given_id;*/

/* All conversations a user is part of */
/*SELECT conversation
FROM Conversation_Participant
WHERE user = given_user*/

/* All users involved in a given conversation */
/*SELECT User.username
FROM Conversation_Participant JOIN User
WHERE Conversation_Participant.user = User.user_id
	AND Conversation_Participant.conversation = given_conversation*/

/*SELECT Direct_Message.content, User.user_name
FROM Direct_Message JOIN User
WHERE Direct_Message.sender = User.user_id
	AND Direct_Message.sender = u1
	AND Direct_Message.recipient = u2;*/