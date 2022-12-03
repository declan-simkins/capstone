package main

import "fmt"

const HEADER_SIZE int = 8

// Header Size: 32 + 16 + 16 = 64 bits -> 8 bytes
type Message struct {
	User_ID      uint32
	Packet_Type  PACKET_TYPE
	Payload_Size uint16
	Payload      string // json object
}

func (this Message) Print_Readable() {
	fmt.Printf("User ID: %d\n", this.User_ID)
	fmt.Printf("Packet Type: %d | %s\n", this.Packet_Type, PACKET_TYPES[this.Packet_Type])
	fmt.Printf("Payload: %s\n", this.Payload)
	fmt.Print("\n")
}

func (this Message) Print_Bytes() {
	Header_Bytes(this.User_ID, uint16(this.Packet_Type), this.Payload_Size)
	Payload_Bytes(this.Payload)
}
