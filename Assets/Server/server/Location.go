package main

import (
	"database/sql"
	"encoding/json"
	"fmt"

	"github.com/solarlune/resolv"
)

var MOVE_SPEED float64 = 2

type collider struct {
	X, Y          float64
	Width, Height float64
	Tag           string
}

type item_stack struct {
	Item   int
	Amount int
}
type pickup_collider struct {
	Collider collider
	Items    []item_stack
}

type patrol_point struct {
	X float32
	Y float32
}
type encounter struct {
	Enemy_ID int
	Amount   int
}
type encounter_collider struct {
	Collider      collider
	Enemies       []encounter
	Patrol_Points []patrol_point
}

type Occupant struct {
	Character_ID int            `json:"character_id"`
	Pos_X        float64        `json:"pos_x"`
	Pos_Y        float64        `json:"pos_y"`
	Obj          *resolv.Object `json:"-"`
}

type trigger_func func(*Occupant)

type trigger struct {
	obj    *resolv.Object
	action trigger_func
}

type Location struct {
	occupants    map[int]*Occupant
	triggers     map[*resolv.Object]*trigger
	map_boundary *resolv.Object
	space        *resolv.Space
}

type scene_data struct {
	Boundary   collider
	Colliders  []collider
	Exits      []collider
	Pickups    []collider
	Encounters []collider
}

func Load_Locations(db *sql.DB) map[int]*Location {
	query := "SELECT location, scene_data " +
		"FROM Location_Scene;"
	rows, err := db.Query(query)
	Check_Error(err, false)

	var locations map[int]*Location = make(map[int]*Location)
	for rows.Next() {
		var location_id int
		var json_colliders string

		rows.Scan(&location_id, &json_colliders)
		if json_colliders == "" {
			continue
		}

		var scene_colliders scene_data
		err = json.Unmarshal([]byte(json_colliders), &scene_colliders)
		Check_Error(err, false)

		locations[location_id] = build_location(scene_colliders)
	}

	return locations
}

func build_location(data scene_data) *Location {
	space := resolv.NewSpace(int(data.Boundary.Width), int(data.Boundary.Height), 1, 1)
	new_location := Location{
		occupants: make(map[int]*Occupant),
		space:     space,
		//map_boundary: boundary_obj,
	}

	for _, c := range data.Colliders {
		new_obj := resolv.NewObject(c.X, c.Y, c.Width, c.Height, c.Tag)
		new_obj.SetShape(resolv.NewRectangle(0, 0, c.Width, c.Height))
		space.Add(new_obj)
	}

	for _, c := range data.Exits {
		new_obj := resolv.NewObject(c.X, c.Y, c.Width, c.Height, c.Tag)
		new_obj.SetShape(resolv.NewRectangle(0, 0, c.Width, c.Height))
		space.Add(new_obj)
	}

	//boundary_obj := resolv.NewObject(data.Boundary.X, data.Boundary.Y, data.Boundary.Width, data.Boundary.Height)

	return &new_location
}

func (this *Location) Add_Occupant(character_id int, start_x, start_y float32) bool {
	// TODO: Should get size from somewhere
	occupant_obj := resolv.NewObject(float64(start_x), float64(start_y), 5, 5)
	occupant_obj.SetShape(resolv.NewRectangle(0, 0, 5, 5))
	this.space.Add(occupant_obj)
	/*
		if intersection := occupant_obj.Shape.Intersection(0, 0, this.map_boundary.Shape); intersection == nil {
			println("Not inside map boundary.")
			return false
		}*/

	new_occupant := Occupant{
		Character_ID: character_id,
		Pos_X:        float64(start_x),
		Pos_Y:        float64(start_y),
		Obj:          occupant_obj,
	}
	this.occupants[character_id] = &new_occupant
	return true
}

func (this *Location) Move_Occupant(character_id int, direction Direction) bool {
	occupant, exists := this.occupants[character_id]
	if !exists {
		return false
	}

	dx := direction.X * (1.0 / float64(REFRESH_RATE)) * MOVE_SPEED
	dy := direction.Y * (1.0 / float64(REFRESH_RATE)) * MOVE_SPEED

	// TODO: Enforce map boundary
	// if intersection := occupant.Obj.Shape.Intersection(dx, dy, this.map_boundary.Shape); intersection == nil {
	// 	return false
	// }

	// Check intersections with all other objects in the space
	for _, other := range occupant.Obj.Space.Objects() {
		intersection := occupant.Obj.Shape.Intersection(dx, dy, other.Shape)
		if intersection != nil {
			tags := other.Tags()
			if len(tags) > 0 && tags[0] == "EXIT" {
				this.Resolve_Exit(occupant, other)
				return false
			} else {
				mtv_x := intersection.MTV.X()
				mtv_y := intersection.MTV.Y()

				if mtv_x != 0 {
					if dx < 0 {
						mtv_x *= -1
					}
					dx = mtv_x
				}
				if mtv_y != 0 {
					if dy < 0 {
						mtv_y *= -1
					}
					dy = mtv_y
				}
			}
		}
	}

	occupant.Obj.X += dx
	occupant.Obj.Y += dy
	occupant.Obj.Update()

	fmt.Printf("Moving Character [%d] p(%f, %f) d(%f, %f).", character_id, occupant.Obj.X, occupant.Obj.Y, dx, dy)

	return true
}

func (this *Location) Resolve_Exit(occupant *Occupant, trigger *resolv.Object) {
	delete(this.occupants, occupant.Character_ID)
	this.space.Remove(occupant.Obj)
}
