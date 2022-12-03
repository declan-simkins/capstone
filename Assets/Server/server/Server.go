package main

import (
	"database/sql"
	"fmt"
	"io"
	"net"
	"strings"
	"time"

	_ "github.com/mattn/go-sqlite3"
	_ "github.com/solarlune/resolv"
)

const VERBOSE bool = true
const DEBUG bool = false

const RESPONSE_STRING string = "Hello, World!"
const DB_PATH string = "../sql/game.db?_foreign_keys=on"
const DB_DRIVER string = "sqlite3"
const REFRESH_RATE int = 30

type Server struct {
	port         string
	conn_type    string
	refresh_rate int
	verbose      bool
	debug        bool
	db           *sql.DB
	Connections  map[uint32]*Connection
	Locations    map[int]*Location
	Equipment    map[int]*Equipment
}

type Connection struct {
	conn             net.Conn
	db               *sql.DB
	user_id          uint32
	active_character *Character
	server           *Server
}

type PACKET_TYPE uint16

// PACKET_TYPE enum
const (
	LOGIN                           PACKET_TYPE = iota
	LOGIN_RESPONSE                  PACKET_TYPE = iota
	GET_CHARACTER_PREVIEWS          PACKET_TYPE = iota
	GET_CHARACTER_PREVIEWS_RESPONSE PACKET_TYPE = iota
	SELECT_CHARACTER                PACKET_TYPE = iota
	SELECT_CHARACTER_RESPONSE       PACKET_TYPE = iota
	GET_FRIENDS                     PACKET_TYPE = iota
	GET_FRIENDS_RESPONSE            PACKET_TYPE = iota
	ADD_FRIEND                      PACKET_TYPE = iota
	ADD_FRIEND_RESPONSE             PACKET_TYPE = iota
	GET_DMS                         PACKET_TYPE = iota
	GET_DMS_RESPONSE                PACKET_TYPE = iota
	SEND_DM                         PACKET_TYPE = iota
	TRAVEL                          PACKET_TYPE = iota
	TRAVEL_RESPONSE                 PACKET_TYPE = iota
	ENTER_LOCATION                  PACKET_TYPE = iota
	ENTER_LOCATION_RESPONSE         PACKET_TYPE = iota
	MOVE_PLAYER                     PACKET_TYPE = iota
	UPDATE_POSITIONS                PACKET_TYPE = iota
	CREATE_ACCOUNT                  PACKET_TYPE = iota
	CREATE_ACCOUNT_RESPONSE         PACKET_TYPE = iota
	CREATE_CHARACTER                PACKET_TYPE = iota
	CREATE_CHARACTER_RESPONSE       PACKET_TYPE = iota
	EXIT_LOCATION                   PACKET_TYPE = iota
	EQUIP                           PACKET_TYPE = iota
	EQUIP_RESPONSE                  PACKET_TYPE = iota
	GET_CHARACTER                   PACKET_TYPE = iota
	GET_CHARACTER_RESPONSE          PACKET_TYPE = iota
)

var PACKET_TYPES map[PACKET_TYPE]string = map[PACKET_TYPE]string{
	LOGIN:                           "LOGIN",
	LOGIN_RESPONSE:                  "LOGIN_RESPONSE",
	GET_CHARACTER_PREVIEWS:          "GET_CHARACTER_PREVIEWS",
	GET_CHARACTER_PREVIEWS_RESPONSE: "GET_CHARACTER_PREVIEWS_RESPONSE",
	SELECT_CHARACTER:                "SELECT_CHARACTER",
	SELECT_CHARACTER_RESPONSE:       "SELECT_CHARACTERS_RESPONSE",
	GET_FRIENDS:                     "GET_FRIENDS",
	GET_FRIENDS_RESPONSE:            "GET_FRIENDS_RESPONSE",
	ADD_FRIEND:                      "ADD_FRIEND",
	ADD_FRIEND_RESPONSE:             "ADD_FRIEND_RESPONSE",
	GET_DMS:                         "GET_DMS",
	GET_DMS_RESPONSE:                "GET_DMS_RESPONSE",
	SEND_DM:                         "SEND_DM",
	TRAVEL:                          "TRAVEL",
	TRAVEL_RESPONSE:                 "TRAVEL_RESPONSE",
	ENTER_LOCATION:                  "ENTER_LOCATION",
	ENTER_LOCATION_RESPONSE:         "ENTER_LOCATION_RESPONSE",
	MOVE_PLAYER:                     "MOVE_PLAYER",
	UPDATE_POSITIONS:                "UPDATE_POSITIONS",
	CREATE_ACCOUNT:                  "CREATE_ACCOUNT",
	CREATE_ACCOUNT_RESPONSE:         "CREATE_ACCOUNT_RESPONSE",
	CREATE_CHARACTER:                "CREATE_CHARACTER",
	CREATE_CHARACTER_RESPONSE:       "CREATE_CHARACTER_RESPONSE",
	EXIT_LOCATION:                   "EXIT_LOCATION",
	EQUIP:                           "EQUIP",
	EQUIP_RESPONSE:                  "EQUIP_RESPONSE",
	GET_CHARACTER:                   "GET_CHARACTER",
	GET_CHARACTER_RESPONSE:          "GET_CHARACTER_RESPONSE",
}

func main() {
	server := Server{
		port:         "8000",
		conn_type:    "tcp",
		refresh_rate: REFRESH_RATE,
		debug:        DEBUG,
		verbose:      VERBOSE,
		db:           Init_DB(DB_DRIVER, DB_PATH),
		Connections:  map[uint32]*Connection{},
		Locations:    map[int]*Location{},
	}

	server.Start()
}

func (this *Server) Start() {
	tcp_address, e := net.ResolveTCPAddr(this.conn_type, "127.0.0.1:"+this.port)
	Check_Error(e, true)

	tcp_listener, e := net.ListenTCP(this.conn_type, tcp_address)
	Check_Error(e, true)

	this.Locations = Load_Locations(this.db)
	this.Equipment = Load_Equipment(this.db)

	go this.Start_Refresh()

	fmt.Printf("Listening on port [%s]...\n", tcp_address.String())
	for {
		tcp_connection, e := tcp_listener.Accept()
		if e != nil {
			Check_Error(e, false)
			continue
		}

		go this.handle_connection(tcp_connection)
	}
}

func (this *Server) Start_Refresh() {
	update_ticker := time.NewTicker(time.Duration(time.Second / time.Duration(this.refresh_rate)))
	for range update_ticker.C {
		this.Refresh()
	}
}

func (this *Server) Refresh() {
	// Update locations of players who are in a location
	for loc_id, location := range this.Locations {
		for occ_id := range location.occupants {
			user_id := Get_Character_User_Query(this.db, occ_id)
			connection := this.Connections[uint32(user_id)]
			connection.Update_Positions(loc_id)
		}
	}
}

func (this *Server) handle_connection(tcp_connection net.Conn) {
	conn := Connection{
		conn:   tcp_connection,
		db:     this.db,
		server: this,
	}

	defer conn.close_connection()
	fmt.Printf("Connection Received from [%s].\n", tcp_connection.RemoteAddr())

	for {
		var payload strings.Builder
		var pkt_type PACKET_TYPE
		var user_id uint32
		var payload_size uint16

		// Get header (user_id, message_action and payload size)
		var header_bytes []byte = make([]byte, HEADER_SIZE)
		bytes_read, err := tcp_connection.Read(header_bytes)

		if this.verbose {
			fmt.Println("\n---Received Message---")
		}

		if err != io.EOF && bytes_read >= HEADER_SIZE {
			user_id = uint32(Extract_Int(header_bytes, 0, 4))
			pkt_type = PACKET_TYPE(Extract_Int(header_bytes, 4, 2))
			payload_size = uint16(Extract_Int(header_bytes, 6, 2))
		} else {
			if err != nil {
				fmt.Println(err)
				return
			}

			fmt.Println("Invalid Packet.")
			return
		}

		if VERBOSE {
			Header_Info(user_id, pkt_type, payload_size)
		}
		if DEBUG {
			Header_Bytes(user_id, uint16(pkt_type), payload_size)
		}

		// Get Payload
		var payload_bytes []byte = make([]byte, payload_size+1)
		_, err = tcp_connection.Read(payload_bytes)
		if err != io.EOF {
			payload.WriteString(string(payload_bytes))
		}

		message := Message{
			User_ID:      user_id,
			Packet_Type:  pkt_type,
			Payload_Size: uint16(payload.Len()),
			Payload:      payload.String(),
		}

		if VERBOSE {
			fmt.Printf("Payload:\n%s\n", message.Payload)
		}
		if DEBUG {
			Payload_Bytes(message.Payload)
		}

		conn.interpret_message(message)
	}
}

// Check if user is already logged in; if not, map user id to connection
func (this *Server) Add_Login(user_id uint32, conn *Connection) bool {
	if _, exists := this.Connections[user_id]; exists {
		fmt.Printf("User [%d] is already logged in.", user_id)
		return false
	}

	this.Connections[user_id] = conn
	return true
}
