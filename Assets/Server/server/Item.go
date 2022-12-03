package main

import "database/sql"

type ATTRIBUTE_TYPE int

const (
	VITALITY ATTRIBUTE_TYPE = iota
	AGILITY  ATTRIBUTE_TYPE = iota
	AFFINITY ATTRIBUTE_TYPE = iota
)

var ATTRIBUTE_TYPES map[ATTRIBUTE_TYPE]string = map[ATTRIBUTE_TYPE]string{
	VITALITY: "Vitality",
	AGILITY:  "Agility",
	AFFINITY: "Affinity",
}

type Item interface {
	Inspect() (name, desc, info string)
	Activate(target Character)
}

type Item_Stack struct {
	item   Item
	amount int
}

type Equipment struct {
	ID         int
	Name, Desc string
	Equip_Slot int
	Mods       map[int]int // map[attribute_type]amount
}

// Run on startup to load all equipment in DB onto server to
// faciliate and optimize equipping / unequipping
// Returns: map[item_id]item
func Load_Equipment(db *sql.DB) map[int]*Equipment {
	return Get_All_Equipment_Query(db)
}

// func NewEquipment() *Equipment {
// 	new_equipment := Equipment{}
// 	for k := range ATTRIBUTE_TYPES {
// 		new_equipment.mods[k] = 0
// 	}
// 	return &new_equipment
// }

// func (this *Equipment) Inspect() (name, desc string, info map[ATTRIBUTE_TYPE]string) {
// 	name = this.name
// 	desc = this.desc
// 	info = make(map[ATTRIBUTE_TYPE]string)
// 	for mod_type, mod := range this.mods {
// 		mod_name := ATTRIBUTE_TYPES[mod_type]
// 		info[mod_type] = fmt.Sprintf("%s: %d", mod_name, mod)
// 	}
// 	return name, desc, info
// }

func (this *Equipment) Activate(target *Character) {
	for _, att := range target.Attributes {
		att_type := att.Attribute_Type
		mod := this.Mods[int(att_type)]
		att.Current_Value += mod
	}
}

func (this *Equipment) Deactivate(target *Character) {
	for _, att := range target.Attributes {
		att_type := att.Attribute_Type
		mod := this.Mods[int(att_type)]
		att.Current_Value -= mod
	}
}
