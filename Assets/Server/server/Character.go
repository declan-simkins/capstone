package main

type Character_Preview_Data struct {
	Character_ID  uint8
	Name          string
	Location_Name string
	Level         uint8
}

type Character struct {
	Name         string
	Level        int
	Location_ID  int
	Character_ID int
	Attributes   []*Attribute
	Skills       []*Skill
	Resources    []*Resource
	Equipment    map[int]*Equipment // equip slot -> equipment
}

type Attribute struct {
	Attribute_Type uint
	Base_Value     int
	Current_Value  int
}

type Skill struct {
	Skill_Type    uint
	Base_Value    int
	Current_Value int
	XP            int
}

type Resource struct {
	Resource_Type uint
	Max           float32
	Min           float32
	Current       float32
	Regen_Rate    float32
	Regen         bool
}

func (this *Character) Initialize() {
	// Equip all equipment
	for _, item := range this.Equipment {
		item.Activate(this)
	}
}
