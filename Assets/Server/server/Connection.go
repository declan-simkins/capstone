package main

import (
	"encoding/binary"
	"encoding/json"
	"fmt"
)

func (this *Connection) close_connection() {
	this.conn.Close()
	delete(this.server.Connections, this.user_id)
	fmt.Printf("Connection to [%s] closed.\n", this.conn.RemoteAddr())
}

func (this *Connection) send_message(message Message) {
	message_bytes := make([]byte, HEADER_SIZE+int(message.Payload_Size))

	// Build Header
	header_bytes := message_bytes[:8]
	binary.LittleEndian.PutUint32(header_bytes[:4], message.User_ID)
	binary.LittleEndian.PutUint16(header_bytes[4:6], uint16(message.Packet_Type))
	binary.LittleEndian.PutUint16(header_bytes[6:8], message.Payload_Size)

	// Build Payload
	payload_bytes := message_bytes[8:]
	for i, b := range []byte(message.Payload) {
		payload_bytes[i] = b
	}

	// Write to connection
	_, err := this.conn.Write(message_bytes)
	Check_Error(err, false)

	if this.server.verbose && message.Packet_Type != UPDATE_POSITIONS {
		fmt.Println()
		fmt.Println("---Sending Message---")
		fmt.Printf("---%s---\n", PACKET_TYPES[message.Packet_Type])
		message.Print_Readable()

		if this.server.debug {
			message.Print_Bytes()
		}
		fmt.Println()
	}
}

func (this *Connection) send_message_type(pkt_type PACKET_TYPE, payload string) {
	msg := Message{
		User_ID:      this.user_id,
		Packet_Type:  pkt_type,
		Payload:      payload,
		Payload_Size: uint16(len(payload)),
	}
	go this.send_message(msg)
}

// TODO: Should use a map of PACKET_TYPE -> function pointer
func (this *Connection) interpret_message(message Message) {
	switch message.Packet_Type {
	case LOGIN:
		this.login(message.Payload, true)

	case GET_CHARACTER_PREVIEWS:
		this.get_character_previews(message.User_ID)

	case SELECT_CHARACTER:
		this.select_character(message.Payload)

	case GET_FRIENDS:
		this.get_friends(message.User_ID)

	case ADD_FRIEND:
		this.add_friend(message)

	case GET_DMS:
		this.Get_DMs(message)

	case SEND_DM:
		this.Send_DM(message)

	case TRAVEL:
		this.Travel(message)

	case ENTER_LOCATION:
		this.Enter_Location(message)

	case MOVE_PLAYER:
		this.Move_Player(message)

	case CREATE_ACCOUNT:
		this.Create_Account(message)

	case CREATE_CHARACTER:
		this.Create_Character(message)

	case EQUIP:
		this.Equip(message)

	case GET_CHARACTER:
		this.Get_Character(message)
	}
}

type Login_Payload struct {
	Username string
	Password string
}
type Login_Response struct {
	Success bool
	User_ID uint32
}

func (this *Connection) login(json_login_data string, trim bool) {
	if this.server.verbose {
		println("Logging in...")
	}

	// Sometimes an EOF or newline is stuck onto the end of the payload
	var byte_data []byte
	if trim {
		byte_data = []byte(json_login_data[:len(json_login_data)-1])
	} else {
		byte_data = []byte(json_login_data)
	}

	var payload Login_Payload
	err := json.Unmarshal(byte_data, &payload)
	Check_Error(err, false)

	var login_response Login_Response
	login_response.User_ID, login_response.Success = Login_Q(
		this.db, payload.Username, payload.Password,
	)

	if login_response.Success {
		this.user_id = login_response.User_ID
		login_response.Success = this.server.Add_Login(this.user_id, this)

		if VERBOSE && login_response.Success {
			fmt.Printf("Successfully logged in with User ID: [%d].\n", login_response.User_ID)
			fmt.Printf("Set connection user id to %d\n", this.user_id)
		}
	} else if VERBOSE {
		fmt.Printf("Failed to log in; no user exists with username [%s]"+
			" or an incorrect password was entered.\n",
			payload.Username,
		)
	}

	response_bytes, err := json.Marshal(login_response)
	Check_Error(err, false)

	this.send_message_type(LOGIN_RESPONSE, string(response_bytes))
}

type Get_Character_Previews_Response struct {
	Character_Previews []Character_Preview_Data
}

func (this *Connection) get_character_previews(user_id uint32) {
	if this.server.verbose {
		println("Getting character previews...")
	}
	character_previews := Get_Character_Previews_Query(this.db, int(user_id))

	payload_data := Get_Character_Previews_Response{
		Character_Previews: character_previews,
	}
	json_payload_data, err := json.Marshal(payload_data)
	Check_Error(err, false)

	go this.send_message_type(GET_CHARACTER_PREVIEWS_RESPONSE, string(json_payload_data))
}

func (this *Connection) select_character(json_select_data string) {
	if this.server.verbose {
		fmt.Println("Getting character...")
	}
	byte_data := []byte(json_select_data[:len(json_select_data)-1])
	select_data := JSON_To_Dict(string(byte_data))
	character_id := int(select_data["Character_ID"].(float64))

	// TODO: Package the next three lines into a method
	char_data := Get_Character_Query(this.db, character_id, this.server)
	this.active_character = &char_data
	this.active_character.Initialize()

	char_json, err := json.Marshal(char_data)
	Check_Error(err, false)

	if VERBOSE {
		fmt.Println("\nCharacter Selected: ")
		fmt.Println(string(char_json))
		fmt.Println()
	}

	this.send_message_type(SELECT_CHARACTER_RESPONSE, string(char_json))
}

type User_Info struct {
	Username string
	User_ID  uint32
}
type Get_Friends_Response struct {
	Users []User_Info
}

func (this *Connection) get_friends(user_id uint32) {
	if this.server.verbose {
		println("Getting friends...")
	}
	response_data := Get_Friends_Response{
		Users: Get_Friends_Query(this.db, user_id),
	}
	json, err := json.Marshal(response_data)
	Check_Error(err, true)
	this.send_message_type(GET_FRIENDS_RESPONSE, string(json))
}

type Add_Friend_Payload struct {
	Friend_ID uint32
}

func (this *Connection) add_friend(msg Message) {
	if this.server.verbose {
		println("Adding friend...")
	}
	add_friend_data := JSON_To_Dict(msg.Payload[:msg.Payload_Size-1])
	add_success := Add_Friend_Query(this.db, this.user_id, uint32(add_friend_data["Friend_ID"].(float64)))

	response_data := struct {
		Success bool
	}{
		Success: add_success,
	}
	json_response, err := json.Marshal(response_data)
	Check_Error(err, true)
	this.send_message_type(ADD_FRIEND_RESPONSE, string(json_response))
}

func (this *Connection) Get_DMs(msg Message) {
	if this.server.verbose {
		println("Getting messages...")
	}
	// Extract participant ids
	get_chat_data := JSON_To_Dict(msg.Payload[:msg.Payload_Size-1])
	participants := make([]int, len(get_chat_data["Participant_IDs"].([]interface{})))
	for i := range get_chat_data["Participant_IDs"].([]interface{}) {
		participants[i] = int(get_chat_data["Participant_IDs"].([]interface{})[i].(float64))
	}

	if len(participants) < 2 {
		return
	}
	this.get_dms_local(participants[0], participants[1])
}

func (this *Connection) get_dms_local(sender, receiver int) {
	// Query DB
	messages, senders := Get_DMs_Query(this.db, sender, receiver)
	if messages == nil || senders == nil {
		return
	}

	// Build and send response
	response_data := struct {
		Messages []string
		Senders  []string
	}{
		Messages: messages,
		Senders:  senders,
	}
	json_response, err := json.Marshal(response_data)
	Check_Error(err, true)
	this.send_message_type(GET_DMS_RESPONSE, string(json_response))
}

func (this *Connection) Send_DM(msg Message) {
	if this.server.verbose {
		println("Sending DM...")
	}
	data := JSON_To_Dict(msg.Payload[:msg.Payload_Size-1])
	sender := int(data["Sender"].(float64))
	recipient := int(data["Recipient"].(float64))
	content := data["Content"].(string)

	Send_DM_Query(this.db, sender, recipient, content)

	// Notify recipient if they are connected
	this.get_dms_local(sender, recipient)
	recipient_conn, exists := this.server.Connections[uint32(recipient)]
	if exists {
		recipient_conn.get_dms_local(recipient, sender)
	}
}

type Travel_Payload struct {
	Character_ID   int
	Destination_ID int
}
type Travel_Response struct {
	Destination_ID int
	Status         bool
}

func (this *Connection) Travel(msg Message) {
	if this.server.verbose {
		println("Travelling...")
	}

	var data Travel_Payload
	err := json.Unmarshal([]byte(msg.Payload[:len(msg.Payload)-1]), &data)
	Check_Error(err, false)

	_, dests := Get_Location_And_Destination_Query(this.db, data.Character_ID)

	var response Travel_Response
	if Linear_Search_Int(dests, data.Destination_ID) == -1 {
		if this.server.verbose {
			fmt.Println("Invalid destination")
		}
		response.Destination_ID = 0
		response.Status = false
	} else {
		Travel_Query(this.db, data.Character_ID, data.Destination_ID)
		response.Destination_ID = data.Destination_ID
		response.Status = true
	}
	json_response, err := json.Marshal(response)
	Check_Error(err, false)
	this.send_message_type(TRAVEL_RESPONSE, string(json_response))
}

type Enter_Location_Payload struct {
	Character_ID int
	Location_ID  int
}
type Enter_Location_Response struct {
	Success    bool
	Scene_Name string
}

func (this *Connection) Enter_Location(msg Message) {
	if this.server.verbose {
		println("Entering Location...")
	}

	trimmed_payload := msg.Payload[:len(msg.Payload)-1]
	var data Enter_Location_Payload
	err := json.Unmarshal([]byte(trimmed_payload), &data)
	Check_Error(err, false)

	var response Enter_Location_Response
	current_location := Get_Location_Query(this.db, data.Character_ID)
	if current_location == data.Location_ID {
		response.Success = true
		response.Scene_Name = Get_Scene_Name_Query(this.db, data.Location_ID)
		location := this.server.Locations[data.Location_ID]
		location.Add_Occupant(data.Character_ID, 0, 0)
	} else {
		response.Success = false
	}

	json_response, err := json.Marshal(response)
	Check_Error(err, false)
	this.send_message_type(ENTER_LOCATION_RESPONSE, string(json_response))
}

type Move_Player_Payload struct {
	Character_ID int       `json:"Character_ID"`
	Direction    Direction `json:"Direction"`
}
type Direction struct {
	X float64 `json:"x"`
	Y float64 `json:"y"`
}

type Exit_Location_Payload struct {
	Character_ID int
	Location_ID  int
}

func (this *Connection) Move_Player(msg Message) {
	if this.server.verbose {
		println("Moving Player...")
	}

	trimmed_payload := msg.Payload[:len(msg.Payload)-1]
	var data Move_Player_Payload
	err := json.Unmarshal([]byte(trimmed_payload), &data)
	Check_Error(err, false)

	current_location := Get_Location_Query(this.db, data.Character_ID)
	exit := !this.server.Locations[current_location].Move_Occupant(
		data.Character_ID, data.Direction)

	if exit {
		exit_payload := Exit_Location_Payload{
			Character_ID: data.Character_ID,
			Location_ID:  current_location,
		}
		json_exit_paylod, err := json.Marshal(exit_payload)
		Check_Error(err, false)
		for _, v := range this.server.Locations[current_location].occupants {
			uid := Get_Character_User_Query(this.db, v.Character_ID)
			this.server.Connections[uint32(uid)].send_message_type(EXIT_LOCATION, string(json_exit_paylod))
		}
		this.send_message_type(EXIT_LOCATION, string(json_exit_paylod))
	}
}

type Occupant_Position struct {
	Character_ID int     `json:"character_id"`
	Pos_X        float64 `json:"pos_x"`
	Pos_Y        float64 `json:"pos_y"`
}

type Update_Occupant_Positions_Payload struct {
	Occupants []Occupant_Position `json:"occupants"`
}

func (this *Connection) Update_Positions(location_id int) {
	if this == nil {
		return
	}
	payload := Update_Occupant_Positions_Payload{
		Occupants: make([]Occupant_Position, 0),
	}
	for _, occupant := range this.server.Locations[location_id].occupants {
		occ_pos := Occupant_Position{
			Character_ID: occupant.Character_ID,
			Pos_X:        occupant.Obj.X,
			Pos_Y:        occupant.Obj.Y,
		}
		payload.Occupants = append(payload.Occupants, occ_pos)
	}
	json_payload, err := json.Marshal(payload)
	Check_Error(err, false)
	this.send_message_type(UPDATE_POSITIONS, string(json_payload))
}

type Create_Account_Payload struct {
	Username string
	Password string
	Confirm  string
}

type Create_Account_Response struct {
	Success bool
	User_ID int
}

func (this *Connection) Create_Account(msg Message) {
	var payload Create_Account_Payload
	err := json.Unmarshal([]byte(msg.Payload[:len(msg.Payload)-1]), &payload)
	Check_Error(err, false)

	var response Create_Account_Response
	if payload.Password != payload.Confirm || len(payload.Password) < 4 {
		response.Success = false
		json_response, err := json.Marshal(response)
		Check_Error(err, false)
		this.send_message_type(CREATE_ACCOUNT_RESPONSE, string(json_response))
		return
	}

	success, id := Create_Account_Query(this.db, payload.Username, payload.Password)
	if success {
		response.Success = true
		response.User_ID = id
	} else {
		response.Success = false
	}
	json_response, err := json.Marshal(response)
	Check_Error(err, false)
	this.send_message_type(CREATE_ACCOUNT_RESPONSE, string(json_response))

	if response.Success {
		login_payload := Login_Payload{
			Username: payload.Username,
			Password: payload.Password,
		}
		json_login_payload, err := json.Marshal(login_payload)
		Check_Error(err, false)
		this.login(string(json_login_payload), false)
	}
}

type Create_Character_Payload struct {
	Character_Name string
}

type Create_Character_Response struct {
	Success      bool
	Character_ID int
}

func (this *Connection) Create_Character(msg Message) {
	var payload Create_Character_Payload
	err := json.Unmarshal([]byte(msg.Payload[:len(msg.Payload)-1]), &payload)
	Check_Error(err, false)

	var response Create_Character_Response
	success, id := Create_Character_Query(this.db, payload.Character_Name, int(this.user_id))
	if success {
		response.Success = true
		response.Character_ID = id
	} else {
		response.Success = false
	}
	json_response, err := json.Marshal(response)
	Check_Error(err, false)
	this.send_message_type(CREATE_CHARACTER_RESPONSE, string(json_response))

	if response.Success {
		// TODO: Package the next three lines into a method
		select_char_response := Get_Character_Query(this.db, id, this.server)
		this.active_character = &select_char_response
		this.active_character.Initialize()

		json_select_char_response, err := json.Marshal(select_char_response)
		Check_Error(err, false)
		this.send_message_type(SELECT_CHARACTER_RESPONSE, string(json_select_char_response))
	}
}

type Equip_Payload struct {
	Character_ID int
	Equipment_ID int
}

func (this *Connection) Equip(msg Message) {
	var payload Equip_Payload
	err := json.Unmarshal([]byte(msg.Payload[:len(msg.Payload)-1]), &payload)
	Check_Error(err, false)

	// TODO: Inventory check
	// if !Check_Inventory_Query(this.db, payload.Character_ID, payload.Equipment_ID) {
	// 	// Send failure response
	// 	return
	// }

	new_item := this.server.Equipment[payload.Equipment_ID]
	old_item, exists := this.active_character.Equipment[new_item.Equip_Slot]
	if exists {
		old_item.Deactivate(this.active_character)
	}
	this.active_character.Equipment[new_item.Equip_Slot] = new_item
	new_item.Activate(this.active_character)

	Update_Character_Equipment(
		this.db,
		this.active_character.Character_ID,
		new_item.ID,
		new_item.Equip_Slot,
	)

	// Don't worry about inventory, just equip stuff
	// Give client a list of all items and they just click an equip button
}

type Get_Character_Payload struct {
	Character_ID int
}

func (this *Connection) Get_Character(msg Message) {
	var payload Get_Character_Payload
	json.Unmarshal([]byte(msg.Payload[:len(msg.Payload)-1]), &payload)

	character := Get_Character_Query(this.db, payload.Character_ID, this.server)
	character.Initialize()
	json_response, err := json.Marshal(character)
	Check_Error(err, false)

	this.send_message_type(GET_CHARACTER_RESPONSE, string(json_response))
}
