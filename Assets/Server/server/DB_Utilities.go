package main

import (
	"database/sql"
	"fmt"
	"time"
)

func Init_DB(db_driver string, db_path string) *sql.DB {
	db, err := sql.Open(db_driver, db_path)
	Check_Error(err, true)

	return db
}

const login_query_string = "" +
	"SELECT [user_id] FROM [User]\n" +
	"WHERE user_name = \"%s\" AND password = \"%s\";"

func Login_Q(db *sql.DB, username, password string) (uid uint32, success bool) {
	query := fmt.Sprintf(login_query_string, username, password)

	var user_id uint32
	row := db.QueryRow(query)
	err := row.Scan(&user_id)

	if err == sql.ErrNoRows {
		return 0, false
	}
	return user_id, true
}

const get_char_previews_query_string = "" +
	"SELECT Player_Character.character_id, Player_Character.name," +
	" Player_Character.level, Location.name\n" +
	"FROM Player_Character JOIN Location\n" +
	"WHERE Player_Character.location = Location.location_id\n" +
	"AND Player_Character.user_id = %d;"

func Get_Character_Previews_Query(db *sql.DB, user_id int) []Character_Preview_Data {
	query := fmt.Sprintf(get_char_previews_query_string, user_id)

	rows, err := db.Query(query)
	Check_Error(err, true)

	characters := make([]Character_Preview_Data, 0)
	for rows.Next() {
		var character_id uint8
		var name string
		var level uint8
		var location_name string

		rows.Scan(&character_id, &name, &level, &location_name)

		new_character := Character_Preview_Data{
			Character_ID:  character_id,
			Name:          name,
			Level:         level,
			Location_Name: location_name,
		}
		characters = append(characters, new_character)
	}
	return characters
}

const get_char_query_string = "" +
	"SELECT name, level, location\n" +
	"FROM Player_Character\n" +
	"WHERE character_id = %d;"

func Get_Character_Query(db *sql.DB, character_id int, server *Server) Character {
	query := fmt.Sprintf(get_char_query_string, character_id)

	result := db.QueryRow(query)

	var name string
	var level int
	var location_id int
	result.Scan(&name, &level, &location_id)

	char_data := Character{
		Name:         name,
		Level:        level,
		Location_ID:  location_id,
		Character_ID: character_id,
		Attributes:   Character_Attributes_Query(db, character_id),
		Skills:       Character_Skills_Query(db, character_id),
		Resources:    Character_Resources_Query(db, character_id),
		Equipment:    Character_Equipment_Query(db, character_id, server),
	}
	return char_data
}

const char_equipment_query_string = "" +
	"SELECT item FROM Character_Equipment\n" +
	"WHERE character = %d;"

func Character_Equipment_Query(db *sql.DB, character_id int, server *Server) map[int]*Equipment {
	query := fmt.Sprintf(char_equipment_query_string, character_id)

	rows, err := db.Query(query)
	Check_Error(err, false)

	equipment := map[int]*Equipment{}
	for rows.Next() {
		var id int

		rows.Scan(&id)
		item, exists := server.Equipment[id]
		if !exists {
			continue
		}
		equipment[item.Equip_Slot] = item
	}
	return equipment
}

const char_attributes_query_string = "" +
	"SELECT attribute_type, base_value, current_value\n" +
	"FROM Attribute\n" +
	"WHERE character = %d;"

func Character_Attributes_Query(db *sql.DB, character_id int) []*Attribute {
	query := fmt.Sprintf(char_attributes_query_string, character_id)

	rows, err := db.Query(query)
	Check_Error(err, true)

	attributes := make([]*Attribute, 0)
	for rows.Next() {
		var a_type uint
		var base int
		var current int

		rows.Scan(&a_type, &base, &current)
		new_attribute := Attribute{
			Attribute_Type: a_type,
			Base_Value:     base,
			Current_Value:  current,
		}

		attributes = append(attributes, &new_attribute)
	}
	return attributes
}

const char_skills_query_string = "" +
	"SELECT skill_type, base_value, current_value, xp\n" +
	"FROM Skill\n" +
	"WHERE character = %d;"

func Character_Skills_Query(db *sql.DB, character_id int) []*Skill {
	query := fmt.Sprintf(char_skills_query_string, character_id)

	rows, err := db.Query(query)
	Check_Error(err, true)

	skills := make([]*Skill, 0)
	for rows.Next() {
		var s_type uint
		var base int
		var current int

		rows.Scan(&s_type, &base, &current)
		new_skill := Skill{
			Skill_Type:    s_type,
			Base_Value:    base,
			Current_Value: current,
		}

		skills = append(skills, &new_skill)
	}
	return skills
}

const char_resources_query_string = "" +
	"SELECT resource_type, max, min, current_value, regen_rate, regen\n" +
	"FROM Resource\n" +
	"WHERE character = %d;"

func Character_Resources_Query(db *sql.DB, character_id int) []*Resource {
	query := fmt.Sprintf(char_resources_query_string, character_id)

	rows, err := db.Query(query)
	Check_Error(err, true)

	resources := make([]*Resource, 0)
	for rows.Next() {
		var r_type uint
		var max, min, current, regen_rate float32
		var regen bool

		rows.Scan(&r_type, &max, &min, &current, &regen_rate, &regen)
		new_resource := Resource{
			Resource_Type: r_type,
			Max:           max,
			Min:           min,
			Current:       current,
			Regen_Rate:    regen_rate,
			Regen:         regen,
		}

		resources = append(resources, &new_resource)
	}
	return resources
}

const get_friends_query_string = "" +
	"SELECT user_name, friend_id " +
	"FROM (" +
	"	SELECT user2 as friend_id " +
	"	FROM Friend " +
	"	WHERE user1 = %d) " +
	"JOIN User " +
	"WHERE user_id = friend_id;"

func Get_Friends_Query(db *sql.DB, user_id uint32) []User_Info {
	friends := make([]User_Info, 0)

	query := fmt.Sprintf(get_friends_query_string, user_id)

	rows, err := db.Query(query)
	Check_Error(err, true)

	for rows.Next() {
		var user_name string
		var user_id int

		rows.Scan(&user_name, &user_id)
		new_friend := User_Info{
			Username: user_name,
			User_ID:  uint32(user_id),
		}

		friends = append(friends, new_friend)
	}
	return friends
}

const add_friend_query_string = "" +
	"INSERT INTO Friend (user1, user2)\n" +
	"VALUES\n" +
	"(%d, %d),\n" +
	"(%d, %d);"

func Add_Friend_Query(db *sql.DB, user1 uint32, user2 uint32) bool {
	if user1 == user2 {
		return false
	}
	query := fmt.Sprintf(add_friend_query_string, user1, user2, user2, user1)

	_, err := db.Exec(query)
	Check_Error(err, false)
	return err == nil
}

const get_dms_query_string = "" +
	"SELECT Direct_Message.content, User.user_name\n" +
	"FROM Direct_Message JOIN User\n" +
	"WHERE Direct_Message.sender = User.user_id\n" +
	"AND Direct_Message.sender IN (%d, %d)\n" +
	"AND Direct_Message.recipient IN (%d, %d)\n" +
	"ORDER BY Direct_Message.time_sent ASC;"

// TODO: Should send back a 'message' struct rather than parallel lists
func Get_DMs_Query(db *sql.DB, sender int, receiver int) ([]string, []string) {
	query := fmt.Sprintf(get_dms_query_string,
		sender, receiver, sender, receiver,
	)

	rows, err := db.Query(query)
	Check_Error(err, false)

	messages := make([]string, 0)
	senders := make([]string, 0)
	for rows.Next() {
		var content string
		var username string

		rows.Scan(&content, &username)
		messages = append(messages, content)
		senders = append(senders, username)
	}

	return messages, senders
}

const send_dm_query_string = "" +
	"INSERT INTO Direct_Message (sender, recipient, content, time_sent)\n" +
	"VALUES (%d, %d, \"%s\", \"%s\");"

func Send_DM_Query(db *sql.DB, sender int, recipient int, content string) {
	time_sent := time.Now().UTC().Format(time.RFC3339)
	query := fmt.Sprintf(send_dm_query_string,
		sender, recipient, content, time_sent,
	)

	_, err := db.Exec(query)
	Check_Error(err, false)
}

const get_loc_and_dest_query_string = "" +
	"SELECT Location.location_id, Location_Destination.destination " +
	"FROM Location " +
	"JOIN Location_Destination ON Location.location_id = Location_Destination.location " +
	"JOIN Player_Character ON Location.location_id = Player_Character.location " +
	"WHERE Player_Character.character_id = %d " +
	"AND Location.location_id = Player_Character.location;"

func Get_Location_And_Destination_Query(db *sql.DB, character_id int) (location int, destinations []int) {
	query := fmt.Sprintf(get_loc_and_dest_query_string, character_id)

	rows, err := db.Query(query)
	Check_Error(err, false)

	destinations = make([]int, 0)
	for rows.Next() {
		var dest int
		rows.Scan(&location, &dest)
		destinations = append(destinations, dest)
	}
	return location, destinations
}

const travel_query_string = "" +
	"UPDATE Player_Character\n" +
	"SET location = %d\n" +
	"WHERE character_id = %d;"

func Travel_Query(db *sql.DB, character_id int, destination_id int) {
	query := fmt.Sprintf(travel_query_string, destination_id, character_id)

	_, err := db.Exec(query)
	Check_Error(err, false)
}

const get_location_query_string = "" +
	"SELECT location\n" +
	"FROM Player_Character\n" +
	"WHERE character_id = %d;"

func Get_Location_Query(db *sql.DB, character_id int) int {
	query := fmt.Sprintf(get_location_query_string, character_id)

	row := db.QueryRow(query)
	var location int
	row.Scan(&location)

	return location
}

const get_scene_name_query_string = "" +
	"SELECT scene_name\n" +
	"FROM Location_Scene\n" +
	"WHERE location = %d;"

func Get_Scene_Name_Query(db *sql.DB, location_id int) string {
	query := fmt.Sprintf(get_scene_name_query_string, location_id)

	row := db.QueryRow(query)
	var scene_name string
	row.Scan(&scene_name)

	return scene_name
}

const get_character_user_query_string = "" +
	"SELECT User.user_id\n" +
	"FROM User JOIN Player_Character\n" +
	"WHERE User.user_id = Player_Character.user_id\n" +
	"AND Player_Character.character_id = %d;"

func Get_Character_User_Query(db *sql.DB, character_id int) int {
	query := fmt.Sprintf(get_character_user_query_string, character_id)

	result := db.QueryRow(query)
	var user_id int
	result.Scan(&user_id)

	return user_id
}

const create_account_query_string = "" +
	"INSERT INTO User (user_name, password)\n" +
	"VALUES (\"%s\", \"%s\");"

func Create_Account_Query(db *sql.DB, username, password string) (bool, int) {
	query := fmt.Sprintf(create_account_query_string, username, password)
	result, err := db.Exec(query)
	Check_Error(err, false)

	if err != nil {
		return false, 0
	} else {
		new_account_id, err := result.LastInsertId()
		Check_Error(err, false)
		return true, int(new_account_id)
	}
}

const create_inventory_query_string = "" +
	"INSERT INTO Inventory (max_bulk) VALUES (%d);"
const create_character_query_string = "" +
	"INSERT INTO Player_Character (name, user_id, inventory_id, location)\n" +
	"VALUES (\"%s\", %d, %d, %d);"

func Create_Character_Query(db *sql.DB, name string, user_id int) (bool, int) {
	create_inv_query := fmt.Sprintf(create_inventory_query_string, 10)
	result, err := db.Exec(create_inv_query)
	Check_Error(err, false)
	inv_id, err := result.LastInsertId()
	Check_Error(err, false)

	query := fmt.Sprintf(create_character_query_string, name, user_id, inv_id, 1)
	result, err = db.Exec(query)
	Check_Error(err, false)

	if err != nil {
		return false, 0
	} else {
		new_char_id, err := result.LastInsertId()
		Check_Error(err, false)
		return true, int(new_char_id)
	}
}

const check_inventory_query_string = "" +
	"SELECT Item_Slot.item\n" +
	"FROM Player_Character JOIN Item_Slot\n" +
	"WHERE Player_Character.inventory_id = Item_Slot.inventory_id\n" +
	"AND Player_Character.character_id = %d\n" +
	"AND Item_Slot.item_id = %d;"

func Check_Inventory_Query(db *sql.DB, character_id int, equipment_id int) bool {
	query := fmt.Sprintf(check_inventory_query_string, character_id, equipment_id)

	var item_id int
	row := db.QueryRow(query)
	err := row.Scan(&item_id)

	if err == sql.ErrNoRows {
		return false
	}

	return true
}

type Modifier struct {
	Attribute int
	Amount    int
}

const get_item_modifiers_query_string = "" +
	"SELECT attribute, amount FROM Item_Modifiers\n" +
	"WHERE item = %d;"

func Get_Item_Modifiers(db *sql.DB, item_id int) []Modifier {
	query := fmt.Sprintf(get_item_modifiers_query_string, item_id)
	rows, err := db.Query(query)
	Check_Error(err, false)

	modifiers := make([]Modifier, 0)
	for rows.Next() {
		var attribute int
		var amount int

		rows.Scan(&attribute, &amount)
		new_modifier := Modifier{
			Attribute: attribute,
			Amount:    amount,
		}
		modifiers = append(modifiers, new_modifier)
	}
	return modifiers
}

type Item_Data struct {
	Equip_Slot  int
	Name        string
	Description string
}

const get_item_data_query_string = "" +
	"SELECT equip_slot, name, description\n" +
	"FROM Item\n" +
	"WHERE item_id = %d;"

func Get_Item_Data(db *sql.DB, item_id int) Item_Data {
	query := fmt.Sprintf(get_item_data_query_string, item_id)
	row := db.QueryRow(query)

	var equip_slot int
	var name, desc string
	err := row.Scan(&equip_slot, &name, &desc)
	Check_Error(err, false)

	item_data := Item_Data{
		Equip_Slot:  equip_slot,
		Name:        name,
		Description: desc,
	}
	return item_data
}

func Get_Equipped_Item_Data(db *sql.DB, equip_slot int) int {

	return 0
}

const get_all_equipment_query_string = "" +
	"SELECT Item.item_id, Item.equip_slot, Item.name, Item.description,\n" +
	"Item_Modifiers.attribute, Item_Modifiers.amount\n" +
	"FROM Item JOIN Item_Modifiers\n" +
	"WHERE Item.item_id = Item_Modifiers.item;"

func Get_All_Equipment_Query(db *sql.DB) map[int]*Equipment {
	rows, err := db.Query(get_all_equipment_query_string)
	Check_Error(err, false)

	equipment := make(map[int]*Equipment)
	for rows.Next() {
		var id, equip_slot, att, amount int
		var name, desc string
		rows.Scan(&id, &equip_slot, &name, &desc, &att, &amount)

		if val, exists := equipment[id]; exists {
			val.Mods[att] = amount
			continue
		}

		new_equipment := Equipment{
			ID:         id,
			Name:       name,
			Desc:       desc,
			Equip_Slot: equip_slot,
			Mods:       make(map[int]int),
		}
		new_equipment.Mods[att] = amount
		equipment[id] = &new_equipment
	}
	return equipment
}

const update_character_equipment_query_string = "" +
	"INSERT OR REPLACE INTO Character_Equipment (character, item, equip_slot)\n" +
	"VALUES (%d, %d, %d);"

func Update_Character_Equipment(db *sql.DB, character_id int, item_id int, equip_slot int) {
	query := fmt.Sprintf(
		update_character_equipment_query_string,
		character_id,
		item_id,
		equip_slot,
	)

	_, err := db.Exec(query)
	Check_Error(err, false)
}
