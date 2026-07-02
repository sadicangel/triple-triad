extends Control
class_name TriadBoardSlotView

const CardViewScene := preload("res://scenes/CardView.tscn")

var atlas: Texture2D
var slot_index := 0
var snapshot: Dictionary = {}
var card_view: Control
var drop_preview := false


func _ready() -> void:
	custom_minimum_size = TriadCardView.CARD_SIZE
	size = TriadCardView.CARD_SIZE
	mouse_filter = Control.MOUSE_FILTER_IGNORE


func initialize(new_slot_index: int, new_atlas: Texture2D) -> void:
	slot_index = new_slot_index
	atlas = new_atlas
	queue_redraw()


func bind(cell_data: Dictionary, new_atlas: Texture2D) -> void:
	snapshot = cell_data.duplicate(true)
	atlas = new_atlas

	if not bool(snapshot.get("has_card", false)):
		if card_view != null:
			remove_child(card_view)
			card_view.queue_free()
			card_view = null
	else:
		if card_view == null:
			card_view = CardViewScene.instantiate()
			add_child(card_view)

		card_view.setup(atlas)
		card_view.bind(snapshot.get("card", {}), false)
		card_view.set_layout_position(Vector2.ZERO, 10)
		card_view.set_drag_visual_preview(false)

	queue_redraw()


func preview_card(card_data: Dictionary, new_atlas: Texture2D) -> void:
	snapshot = {
		"index": slot_index,
		"element": "None",
		"can_drop": false,
		"has_card": true,
		"card": card_data.duplicate(true),
	}
	atlas = new_atlas

	if card_view == null:
		card_view = CardViewScene.instantiate()
		add_child(card_view)

	card_view.setup(atlas)
	card_view.bind(snapshot["card"], false)
	card_view.set_layout_position(Vector2.ZERO, 10)
	card_view.set_drag_visual_preview(false)
	queue_redraw()


func clear_card_visual() -> void:
	if card_view == null:
		return

	remove_child(card_view)
	card_view.queue_free()
	card_view = null
	snapshot["has_card"] = false
	snapshot.erase("card")
	queue_redraw()


func get_card_view() -> Control:
	return card_view


func can_drop() -> bool:
	return bool(snapshot.get("can_drop", false)) and not bool(snapshot.get("has_card", false))


func get_snapshot() -> Dictionary:
	return snapshot


func hit_test_virtual(virtual_point: Vector2) -> bool:
	return Rect2(position, size).has_point(virtual_point)


func set_drop_preview(enabled: bool) -> void:
	if drop_preview == enabled:
		return

	drop_preview = enabled
	queue_redraw()


func animate_played() -> void:
	if card_view == null:
		return

	card_view.scale = Vector2(0.88, 0.88)
	create_tween().tween_property(card_view, "scale", Vector2.ONE, 0.14).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


func animate_captured() -> void:
	if card_view == null:
		return

	var tween := create_tween()
	tween.tween_property(card_view, "scale", Vector2(1.12, 1.12), 0.10).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	tween.tween_property(card_view, "scale", Vector2.ONE, 0.14).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)


func _draw() -> void:
	var rect := Rect2(Vector2.ZERO, size)
	draw_rect(rect, TriadCardPalette.board_slot(), true)
	draw_rect(rect, TriadCardPalette.board_slot_border(), false, 2.0)

	if atlas != null:
		draw_texture_rect_region(
			atlas,
			rect,
			TriadAtlasRegions.fill(1),
			Color(0.98, 0.88, 0.58, 0.12))

	if drop_preview:
		draw_rect(rect, TriadCardPalette.drop_preview(), true)
		draw_rect(rect.grow(-4), TriadCardPalette.drop_preview_border(), false, 4.0)
	elif can_drop():
		draw_rect(rect.grow(-5), Color(0.62, 1.0, 0.88, 0.22), false, 2.0)
