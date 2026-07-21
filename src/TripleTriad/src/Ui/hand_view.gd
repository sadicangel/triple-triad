extends Control
class_name TriadHandView

signal card_drag_requested(card, source_view: Control)

const CardViewScene := preload("res://scenes/CardView.tscn")
const CARD_STEP_Y := 128.0

var seat := ""
var card_views: Array[Control] = []


func _ready() -> void:
    size = Vector2(TriadCardView.CARD_SIZE.x, TriadCardView.CARD_SIZE.y + CARD_STEP_Y * 4.0)
    custom_minimum_size = size
    mouse_filter = Control.MOUSE_FILTER_IGNORE


func bind(hand_data, atlas: Texture2D) -> void:
    _clear_cards()
    if hand_data == null:
        seat = ""
        return

    seat = _seat_name(hand_data.Seat)

    var cards: Array = hand_data.Cards
    for i in cards.size():
        var card = cards[i]
        var view := CardViewScene.instantiate()
        add_child(view)
        view.setup(atlas)
        view.bind(card, true)
        view.set_layout_position(Vector2(0, i * CARD_STEP_Y), i)
        view.drag_requested.connect(_on_card_drag_requested)
        card_views.append(view)


func get_card_view_at(index: int) -> Control:
    if index < 0 or index >= card_views.size():
        return null

    return card_views[index]


func hide_card_at(index: int) -> void:
    var view := get_card_view_at(index)
    if view != null:
        view.visible = false


func show_card_at(index: int) -> void:
    var view := get_card_view_at(index)
    if view != null:
        view.visible = true


func _clear_cards() -> void:
    for view in card_views:
        if is_instance_valid(view):
            if view.drag_requested.is_connected(_on_card_drag_requested):
                view.drag_requested.disconnect(_on_card_drag_requested)
            remove_child(view)
            view.queue_free()

    card_views.clear()


func _on_card_drag_requested(card, source_view: Control) -> void:
    card_drag_requested.emit(card, source_view)


func _seat_name(value) -> String:
    match str(value):
        "0":
            return "Red"
        "1":
            return "Blue"
        _:
            return str(value)
