extends RefCounted
class_name TriadAtlasRegions

const TILE_SIZE := 256
const COLUMNS := 11

const ELEMENT_COLUMNS := {
	"None": 0,
	"Fire": 1,
	"Ice": 2,
	"Thunder": 3,
	"Water": 4,
	"Earth": 5,
	"Wind": 6,
	"Holy": 7,
	"Dark": 8,
	"Poison": 9,
}

static func card_face(card_number: int) -> Rect2:
	var index: int = max(card_number - 1, 0)
	var row: int = floori(float(index) / float(COLUMNS))
	return _tile(index % COLUMNS, row)


static func fill(fill_index: int) -> Rect2:
	return _tile(fill_index, 10)


static func value(direction: String, rank: int) -> Rect2:
	var row := 11
	match direction:
		"west":
			row = 11
		"north":
			row = 12
		"east":
			row = 13
		"south":
			row = 14
	return _tile(clampi(rank, 0, 10), row)


static func element(element_name: String) -> Rect2:
	return _tile(int(ELEMENT_COLUMNS.get(element_name, 0)), 15)


static func _tile(column: int, row: int) -> Rect2:
	return Rect2(
		Vector2(column * TILE_SIZE, row * TILE_SIZE),
		Vector2(TILE_SIZE, TILE_SIZE))
