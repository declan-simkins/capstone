INSERT INTO User (user_name, password)
VALUES
	("test", 1234),
	("test2", 5678),
	("test3", 2345);

INSERT INTO Inventory (inventory_id)
VALUES
	(1), (2), (3), (4), (5);

INSERT INTO Location (name, description, hidden)
VALUES
	("Shimmerstone", "Clifftop city", 0),
	("Ashenrise", "Scorched town", 0),
	("Barrows", "Haunted Barrows", 1),
	("Crystal River Crossing", "Well maintained bridge", 0);

INSERT INTO Route (name, hidden, length, complexity, danger)
VALUES
	("Bayridge Road (South)", 0, 2, 1, 1),
	("Bayridge Road (South-East)", 0, 2, 1, 1),
	("Grayhill's Walk (South)", 1, 2, 4, 3),
	("Grayhill's Walk (North)", 1, 1, 3, 3),
	("Crystal River Road (South)", 0, 2, 2, 2);

INSERT INTO Location_Destination (location, route, destination)
VALUES
	(1, 1, 2),
	(2, 1, 1), (2, 3, 3), (2, 2, 4),
	(3, 3, 2), (3, 4, 4),
	(4, 2, 2), (4, 4, 3);

INSERT INTO Location_Scene (location, scene_name)
VALUES
	(1, "Shimmerstone"),
	(2, "Ashenrise"),
	(3, "Barrows"),
	(4, "Crystal River Crossing");

INSERT INTO Player_Character (user_id, inventory_id, name, location)
VALUES
	(1, 1, "Test Character 1", 1),
	(1, 2, "Test Character 2", 2),
	(2, 3, "Test2 Character 1", 2),
	(2, 4, "Test2 Character 2", 1),
	(3, 5, "Test3 Character 1", 2);

/* TODO: Create attributes, skills, resources, abilities for some characters */
INSERT INTO Attribute (character, attribute_type, base_value, current_value)
VALUES
	(1, 0, 10, 10),
	(1, 1, 7, 7),
	(1, 2, 5, 5),
	(2, 0, 6, 6),
	(2, 1, 5, 5),
	(2, 2, 13, 13),
	(3, 0, 5, 5),
	(3, 1, 8, 8),
	(3, 2, 15, 15),
	(4, 0, 9, 9),
	(4, 1, 5, 5),
	(4, 2, 11, 11);

INSERT INTO Friend (user1, user2)
VALUES
	(1, 2),
	(2, 1),
	(1, 3),
	(3, 1);

INSERT INTO Conversation (conversation_id)
VALUES
	(1), (2), (3);

INSERT INTO Conversation_Participant (conversation, user)
VALUES
	(1, 1), (1, 2),
	(2, 1), (2, 3),
	(3, 1), (3, 2), (3, 3);

/*INSERT INTO Message (content, conversation, sender)
VALUES
	("Hey", 1, 1), ("How's it going?", 1, 2), ("Good, you?", 1, 1),
	("Hello", 2, 3), ("Howdy", 2, 1),
	("Hey guys", 3, 1), ("Heyo", 3, 2), ("How you doing?", 3, 3), ("Good, thanks", 3, 1);*/

/*INSERT INTO Direct_Message (content, sender, recipient, time_sent)
VALUES
	("Hello!", 1, 2), ("Hi There!", 2, 1);*/