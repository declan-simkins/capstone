package main

type Ability struct {
	Ability_ID          uint32
	Name                string
	Description         string
	Mana_Cost           uint32
	Energy_Cost         uint32
	Stamina_Cost        uint32
	Casting_Time        float32
	Cooldown            float32
	Immobile_Casting    bool
	Immobile_Activation bool
}

// TODO: ACTION_TYPE enum should be defined in database
type ACTION_TYPE int

const (
	MOVE  ACTION_TYPE = iota
	SPAWN ACTION_TYPE = iota
	DELAY ACTION_TYPE = iota
)

type Ability_Action struct {
	Action_ID   uint32
	Action_Type ACTION_TYPE
	Use_Cache   bool
}
