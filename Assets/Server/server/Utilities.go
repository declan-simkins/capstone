package main

import (
	"encoding/json"
	"fmt"
	"os"
)

func Extract_Int(bytes []byte, start_byte int, num_bytes int) int {
	if len(bytes[start_byte:]) < num_bytes {
		return 0
	}

	var value int
	for i := start_byte; i < start_byte+num_bytes; i++ {
		value += int(bytes[i]) << ((i - start_byte) * 8)
	}

	return value
}

func JSON_To_Dict(json_string string) map[string]interface{} {
	var data map[string]interface{}
	err := json.Unmarshal([]byte(json_string), &data)
	Check_Error(err, false)

	return data
}

func Check_Error(e error, exit bool) {
	if e == nil {
		return
	}

	fmt.Fprintf(os.Stderr, "Error: %s", e.Error())

	if exit {
		fmt.Fprintf(os.Stderr, "Exiting...")
		os.Exit(1)
	}
}

func Linear_Search_Int(arr []int, target int) int {
	for i := 0; i < len(arr); i++ {
		if arr[i] == target {
			return i
		}
	}
	return -1
}
