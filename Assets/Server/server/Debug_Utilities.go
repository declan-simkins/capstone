package main

import (
	"encoding/binary"
	"fmt"
)

func Header_Info(user_id uint32, pkt_type PACKET_TYPE, payload_size uint16) {
	fmt.Printf("User ID: %d\n", user_id)
	fmt.Printf("Packet Type: %d | %s\n", pkt_type, PACKET_TYPES[pkt_type])
	fmt.Printf("Payload Size: %d\n", payload_size)
}

func Header_Bytes(user_id uint32, pkt_type uint16, payload_size uint16) {
	fmt.Printf("User ID (Bytes): ")
	temp_bytes := make([]byte, 4)
	binary.LittleEndian.PutUint32(temp_bytes, user_id)
	Debug_Bytes(temp_bytes)
	fmt.Println()

	fmt.Printf("Packet Type (Bytes): ")
	temp_bytes = make([]byte, 4)
	binary.LittleEndian.PutUint16(temp_bytes, pkt_type)
	Debug_Bytes(temp_bytes)
	fmt.Println()

	fmt.Printf("Payload Size (Bytes): ")
	temp_bytes = make([]byte, 4)
	binary.LittleEndian.PutUint16(temp_bytes, payload_size)
	Debug_Bytes(temp_bytes)
	fmt.Println()
}

func Payload_Bytes(payload string) {
	fmt.Printf("Payload (Bytes):\n")
	Debug_Bytes([]byte(payload))
	fmt.Println()
}

func Debug_Bytes(bytes []byte) {
	fmt.Printf("|")
	for i := 0; i < len(bytes); i++ {
		fmt.Printf("0x%02X|", bytes[i])
	}
}
