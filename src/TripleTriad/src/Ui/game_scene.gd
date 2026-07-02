extends Control

const BoardSlotScene := preload("res://scenes/BoardSlotView.tscn")
const CardViewScene := preload("res://scenes/CardView.tscn")
const HandViewScene := preload("res://scenes/HandView.tscn")
const VIRTUAL_SIZE := Vector2(1440, 810)

var atlas: Texture2D
var bridge: Node
var full_background: ColorRect
var virtual_root: Control
var red_hand: Control
var blue_hand: Control
var slots: Array[Control] = []
var score_label: Label
var status_label: Label
var drag_layer: Control
var drag_view: Control
var dragged_card: Dictionary = {}
var drag_origin := Vector2.ZERO
var drag_offset := Vector2.ZERO
var virtual_scale := 1.0
var is_submitting := false


func _ready() -> void:
    atlas = load("res://assets/triple_triad/spritesheet.png")
    bridge = $GameSessionBridge
    _build_scene()
    _update_virtual_layout()
    bridge.snapshot_changed.connect(_on_snapshot_changed)
    bridge.game_event_raised.connect(_on_game_event_raised)
    _apply_snapshot(bridge.get_current_snapshot())


func _notification(what: int) -> void:
    if what == NOTIFICATION_RESIZED and virtual_root != null:
        _update_virtual_layout()


func _process(_delta: float) -> void:
    if drag_view == null:
        return

    var virtual_mouse := _screen_to_virtual(get_global_mouse_position())
    drag_view.position = virtual_mouse - drag_offset

    var hover_slot := _find_drop_slot(virtual_mouse)
    for slot in slots:
        slot.set_drop_preview(slot == hover_slot)


func _input(event: InputEvent) -> void:
    if drag_view == null:
        return

    if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and not event.pressed:
        get_viewport().set_input_as_handled()
        _finish_drag()


func _build_scene() -> void:
    full_background = ColorRect.new()
    full_background.name = "Background"
    full_background.color = TriadCardPalette.background()
    full_background.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(full_background)

    virtual_root = Control.new()
    virtual_root.name = "VirtualRoot"
    virtual_root.size = VIRTUAL_SIZE
    virtual_root.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(virtual_root)

    var play_mat := ColorRect.new()
    play_mat.name = "PlayMat"
    play_mat.color = Color(0.045, 0.055, 0.065)
    play_mat.size = VIRTUAL_SIZE
    play_mat.mouse_filter = Control.MOUSE_FILTER_IGNORE
    virtual_root.add_child(play_mat)

    var board_panel := ColorRect.new()
    board_panel.name = "BoardPanel"
    board_panel.color = TriadCardPalette.board_panel()
    board_panel.position = Vector2(328, 16)
    board_panel.size = Vector2(784, 784)
    board_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
    virtual_root.add_child(board_panel)

    red_hand = _create_hand_view("RedHand", Vector2(24, 24))
    blue_hand = _create_hand_view("BlueHand", Vector2(1160, 24))

    for index in 9:
        slots.append(_create_board_slot(index))

    score_label = _create_label("ScoreLabel", Vector2(520, 0), Vector2(400, 28), 22)
    status_label = _create_label("StatusLabel", Vector2(456, 778), Vector2(528, 28), 22)

    drag_layer = Control.new()
    drag_layer.name = "DragLayer"
    drag_layer.size = VIRTUAL_SIZE
    drag_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
    drag_layer.z_index = 3000
    virtual_root.add_child(drag_layer)


func _create_hand_view(node_name: String, node_position: Vector2) -> Control:
    var hand := HandViewScene.instantiate()
    hand.name = node_name
    hand.position = node_position
    hand.z_index = 100
    hand.card_drag_requested.connect(_start_drag)
    virtual_root.add_child(hand)
    return hand


func _create_board_slot(index: int) -> Control:
    var slot := BoardSlotScene.instantiate()
    slot.name = "BoardSlot_%s" % index
    slot.position = Vector2(336 + index % 3 * 256, 24 + index / 3 * 256)
    slot.z_index = 50
    slot.initialize(index, atlas)
    virtual_root.add_child(slot)
    return slot


func _create_label(node_name: String, node_position: Vector2, node_size: Vector2, font_size: int) -> Label:
    var label := Label.new()
    label.name = node_name
    label.position = node_position
    label.size = node_size
    label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    label.mouse_filter = Control.MOUSE_FILTER_IGNORE
    label.z_index = 2000
    label.add_theme_font_size_override("font_size", font_size)
    label.add_theme_color_override("font_color", TriadCardPalette.text())
    virtual_root.add_child(label)
    return label


func _apply_snapshot(snapshot: Dictionary) -> void:
    if snapshot.is_empty():
        return

    var red_hand_data := {}
    var blue_hand_data := {}
    for hand in snapshot.get("hands", []):
        if str(hand.get("seat", "")) == "Red":
            red_hand_data = hand
        elif str(hand.get("seat", "")) == "Blue":
            blue_hand_data = hand

    red_hand.bind(red_hand_data, atlas)
    blue_hand.bind(blue_hand_data, atlas)

    var board: Array = snapshot.get("board", [])
    for i in min(slots.size(), board.size()):
        slots[i].bind(board[i], atlas)

    score_label.text = "RED %s     BLUE %s" % [int(snapshot.get("red_score", 0)), int(snapshot.get("blue_score", 0))]
    status_label.text = _format_winner(snapshot) if bool(snapshot.get("is_complete", false)) else "%s TURN     %s" % [
        str(snapshot.get("active_seat", "")).to_upper(),
        " / ".join(snapshot.get("rules", [])),
    ]


func _format_winner(snapshot: Dictionary) -> String:
    var blue_score := int(snapshot.get("blue_score", 0))
    var red_score := int(snapshot.get("red_score", 0))
    if blue_score == red_score:
        return "DRAW"
    return "BLUE WINS" if blue_score > red_score else "RED WINS"


func _start_drag(card: Dictionary, source_view: Control) -> void:
    if drag_view != null or is_submitting or not bool(card.get("playable", false)):
        return

    dragged_card = card.duplicate(true)
    drag_origin = _screen_to_virtual(source_view.get_global_rect().position)
    var virtual_mouse := _screen_to_virtual(get_global_mouse_position())
    drag_offset = virtual_mouse - drag_origin

    var face_card := dragged_card.duplicate(true)
    face_card["face_up"] = true
    face_card["playable"] = false

    drag_view = CardViewScene.instantiate()
    drag_view.name = "DraggedCard"
    drag_view.z_index = 3500
    drag_layer.add_child(drag_view)
    drag_view.setup(atlas)
    drag_view.bind(face_card, false)
    drag_view.set_drag_visual_preview(true)
    drag_view.position = drag_origin
    status_label.text = "PLACE CARD"


func _finish_drag() -> void:
    var current_drag_view := drag_view
    var current_card := dragged_card
    if current_drag_view == null or current_card.is_empty():
        return

    var virtual_mouse := _screen_to_virtual(get_global_mouse_position())
    var drop_slot := _find_drop_slot(virtual_mouse)
    _clear_drop_previews()

    if drop_slot == null:
        _snap_back_drag_view(current_drag_view)
        drag_view = null
        dragged_card = {}
        return

    var slot_snapshot: Dictionary = drop_slot.get_snapshot()
    var request_id := "%s-%s" % [Time.get_ticks_usec(), randi()]
    drag_view = null
    dragged_card = {}
    _animate_drag_view_to_slot(current_drag_view, drop_slot)
    is_submitting = true
    bridge.submit_play_card(str(current_card.get("id", "")), int(slot_snapshot.get("index", -1)), request_id)
    is_submitting = false


func _find_drop_slot(virtual_mouse: Vector2) -> Control:
    for slot in slots:
        if slot.can_drop() and slot.hit_test_virtual(virtual_mouse):
            return slot
    return null


func _clear_drop_previews() -> void:
    for slot in slots:
        slot.set_drop_preview(false)


func _animate_drag_view_to_slot(view: Control, slot: Control) -> void:
    var tween := create_tween()
    tween.tween_property(view, "position", slot.position, 0.12).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    tween.tween_callback(view.queue_free)


func _snap_back_drag_view(view: Control) -> void:
    var tween := create_tween()
    tween.tween_property(view, "position", drag_origin, 0.14).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    tween.tween_callback(view.queue_free)
    _apply_snapshot(bridge.get_current_snapshot())


func _on_snapshot_changed(snapshot: Dictionary) -> void:
    _apply_snapshot(snapshot)


func _on_game_event_raised(game_event: Dictionary) -> void:
    match str(game_event.get("type", "")):
        "card_played":
            var played_slot := int(game_event.get("board_slot_index", -1))
            if played_slot >= 0 and played_slot < slots.size():
                slots[played_slot].animate_played()
        "card_captured":
            var captured_slot := int(game_event.get("board_slot_index", -1))
            if captured_slot >= 0 and captured_slot < slots.size():
                slots[captured_slot].animate_captured()
        "move_rejected":
            status_label.text = str(game_event.get("reason", "")).to_upper()
        "turn_changed":
            status_label.text = "%s TURN" % str(game_event.get("active_seat", "")).to_upper()
        "match_ended":
            status_label.text = _format_winner(bridge.get_current_snapshot())


func _screen_to_virtual(screen_position: Vector2) -> Vector2:
    if virtual_scale <= 0.0:
        return screen_position
    return (screen_position - virtual_root.global_position) / virtual_scale


func _update_virtual_layout() -> void:
    var viewport_size := get_viewport_rect().size
    if full_background != null:
        full_background.size = viewport_size

    virtual_scale = min(viewport_size.x / VIRTUAL_SIZE.x, viewport_size.y / VIRTUAL_SIZE.y)
    virtual_root.scale = Vector2(virtual_scale, virtual_scale)
    virtual_root.position = (viewport_size - VIRTUAL_SIZE * virtual_scale) * 0.5
