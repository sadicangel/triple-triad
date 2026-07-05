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
var session_connected := false
var has_snapshot := false
var is_submitting := false
var animation_queue: Array[Dictionary] = []
var animations_running := false
var pending_snapshot: Dictionary = {}
var submitted_drag_views: Dictionary = {}


func _ready() -> void:
    atlas = load("res://assets/triple_triad/spritesheet.png")
    bridge = $GameSessionBridge
    _build_scene()
    _update_virtual_layout()
    bridge.snapshot_changed.connect(_on_snapshot_changed)
    bridge.game_event_raised.connect(_on_game_event_raised)
    bridge.connection_state_changed.connect(_on_connection_state_changed)
    var initial_snapshot: Dictionary = bridge.get_current_snapshot()
    if initial_snapshot.is_empty():
        status_label.text = "CONNECTING"
    else:
        _apply_snapshot(initial_snapshot)


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

    has_snapshot = true
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
    if drag_view != null or is_submitting or not _session_ready() or not bool(card.get("playable", false)):
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

    if drop_slot == null or not _session_ready():
        _snap_back_drag_view(current_drag_view)
        drag_view = null
        dragged_card = {}
        return

    var slot_snapshot: Dictionary = drop_slot.get_snapshot()
    var request_id := "%s-%s" % [Time.get_ticks_usec(), randi()]
    drag_view = null
    dragged_card = {}
    submitted_drag_views[request_id] = {
        "view": current_drag_view,
        "origin": drag_origin,
    }
    is_submitting = true
    bridge.submit_play_card(str(current_card.get("id", "")), int(slot_snapshot.get("index", -1)), request_id)


func _find_drop_slot(virtual_mouse: Vector2) -> Control:
    if not _session_ready():
        return null

    for slot in slots:
        if slot.can_drop() and slot.hit_test_virtual(virtual_mouse):
            return slot
    return null


func _clear_drop_previews() -> void:
    for slot in slots:
        slot.set_drop_preview(false)


func _animate_card_view_to_slot(view: Control, slot: Control) -> void:
    view.z_index = 3500
    var start_position := view.position
    var midpoint := (start_position + slot.position) * 0.5 + Vector2(0, -44)

    var lift := create_tween()
    lift.set_parallel(true)
    lift.tween_property(view, "position", midpoint, 0.14).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    lift.tween_property(view, "scale", Vector2(1.05, 1.05), 0.14).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    await lift.finished

    var settle := create_tween()
    settle.set_parallel(true)
    settle.tween_property(view, "position", slot.position, 0.16).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
    settle.tween_property(view, "scale", Vector2.ONE, 0.16).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
    await settle.finished


func _snap_back_drag_view(view: Control) -> void:
    var tween := create_tween()
    tween.tween_property(view, "position", drag_origin, 0.14).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    tween.tween_callback(view.queue_free)
    _apply_snapshot(bridge.get_current_snapshot())


func _on_snapshot_changed(snapshot: Dictionary) -> void:
    if animations_running or not animation_queue.is_empty():
        pending_snapshot = snapshot.duplicate(true)
        return

    _apply_snapshot(snapshot)


func _on_game_event_raised(game_event: Dictionary) -> void:
    match str(game_event.get("type", "")):
        "card_played":
            _clear_submitting_if_local(game_event)
            _enqueue_animation(game_event)
        "card_captured":
            _enqueue_animation(game_event)
        "move_rejected":
            _clear_submitting_if_local(game_event)
            _handle_move_rejected(game_event)
            status_label.text = str(game_event.get("reason", "")).to_upper()
        "turn_changed":
            status_label.text = "%s TURN" % str(game_event.get("active_seat", "")).to_upper()
        "match_ended":
            status_label.text = _format_winner(bridge.get_current_snapshot())


func _on_connection_state_changed(connection_state: Dictionary) -> void:
    var state := str(connection_state.get("state", ""))
    session_connected = state == "Connected"

    if session_connected:
        if not has_snapshot:
            status_label.text = "CONNECTING"
        return

    _clear_drop_previews()
    if drag_view != null:
        _snap_back_drag_view(drag_view)
        drag_view = null
        dragged_card = {}

    if state == "Failed" or state == "Disconnected" or state == "Closed":
        is_submitting = false
        for request_id in submitted_drag_views.keys():
            var entry: Dictionary = submitted_drag_views[request_id]
            var view: Control = entry.get("view", null)
            if view != null and is_instance_valid(view):
                drag_origin = entry.get("origin", view.position)
                _snap_back_drag_view(view)
        submitted_drag_views.clear()

    match state:
        "Connecting", "Reconnecting":
            status_label.text = state.to_upper()
        "Failed":
            status_label.text = str(connection_state.get("reason", "SESSION FAILED")).to_upper()
        "Disconnected", "Closed":
            status_label.text = state.to_upper()


func _session_ready() -> bool:
    return session_connected and has_snapshot


func _enqueue_animation(game_event: Dictionary) -> void:
    animation_queue.append(game_event.duplicate(true))
    _drain_animation_queue()


func _drain_animation_queue() -> void:
    if animations_running:
        return

    animations_running = true
    while not animation_queue.is_empty():
        var game_event: Dictionary = animation_queue.pop_front()
        match str(game_event.get("type", "")):
            "card_played":
                await _animate_card_played(game_event)
            "card_captured":
                await _animate_card_captured(game_event)

    animations_running = false
    if not pending_snapshot.is_empty():
        var snapshot_to_apply := pending_snapshot.duplicate(true)
        pending_snapshot = {}
        _apply_snapshot(snapshot_to_apply)


func _animate_card_played(game_event: Dictionary) -> void:
    var slot_index := int(game_event.get("board_slot_index", -1))
    if slot_index < 0 or slot_index >= slots.size():
        return

    var slot: Control = slots[slot_index]
    var card_data: Dictionary = game_event.get("card", {})
    if card_data.is_empty():
        return

    card_data = card_data.duplicate(true)
    card_data["face_up"] = true
    card_data["playable"] = false

    var source_seat := str(game_event.get("source_seat", ""))
    var source_hand_index := int(game_event.get("source_hand_index", -1))
    var source_hand := _hand_for_seat(source_seat)
    if source_hand != null:
        source_hand.hide_card_at(source_hand_index)

    var request_id := str(game_event.get("client_request_id", ""))
    var view := _take_submitted_drag_view(request_id)
    if view == null:
        view = _create_flying_card_from_hand(card_data, source_hand, source_hand_index)

    if view == null:
        slot.preview_card(card_data, atlas)
        slot.animate_played()
        return

    await _animate_card_view_to_slot(view, slot)
    slot.preview_card(card_data, atlas)
    slot.animate_played()
    view.queue_free()


func _animate_card_captured(game_event: Dictionary) -> void:
    var slot_index := int(game_event.get("board_slot_index", -1))
    if slot_index < 0 or slot_index >= slots.size():
        return

    var slot: Control = slots[slot_index]
    var card_data: Dictionary = game_event.get("card", {})
    if card_data.is_empty():
        return

    var card_view: Control = slot.get_card_view()
    if card_view == null:
        slot.preview_card(card_data, atlas)
        card_view = slot.get_card_view()

    if card_view == null:
        return

    var previous_owner := str(game_event.get("previous_owner", ""))
    var new_owner := str(game_event.get("new_owner", ""))
    var flip_sign := _flip_sign_for_owner(new_owner)
    await card_view.animate_owner_flip(previous_owner, new_owner, flip_sign)
    slot.preview_card(card_data, atlas)


func _take_submitted_drag_view(request_id: String) -> Control:
    if request_id.is_empty() or not submitted_drag_views.has(request_id):
        return null

    var entry: Dictionary = submitted_drag_views[request_id]
    submitted_drag_views.erase(request_id)
    var view: Control = entry.get("view", null)
    return view if is_instance_valid(view) else null


func _create_flying_card_from_hand(card_data: Dictionary, source_hand: Control, source_hand_index: int) -> Control:
    var source_view: Control = null
    if source_hand != null:
        source_view = source_hand.get_card_view_at(source_hand_index)

    var start_position := Vector2.ZERO
    if source_view != null:
        start_position = _screen_to_virtual(source_view.get_global_rect().position)
    elif source_hand != null:
        start_position = source_hand.position + Vector2(0, max(source_hand_index, 0) * 128.0)

    var view := CardViewScene.instantiate()
    view.name = "PlayedCard"
    view.z_index = 3500
    drag_layer.add_child(view)
    view.setup(atlas)
    view.bind(card_data, false)
    view.set_drag_visual_preview(true)
    view.position = start_position
    return view


func _handle_move_rejected(game_event: Dictionary) -> void:
    var request_id := str(game_event.get("client_request_id", ""))
    if request_id.is_empty() or not submitted_drag_views.has(request_id):
        return

    var entry: Dictionary = submitted_drag_views[request_id]
    submitted_drag_views.erase(request_id)
    var view: Control = entry.get("view", null)
    if view == null or not is_instance_valid(view):
        return

    drag_origin = entry.get("origin", view.position)
    _snap_back_drag_view(view)


func _clear_submitting_if_local(game_event: Dictionary) -> void:
    var request_id := str(game_event.get("client_request_id", ""))
    if not request_id.is_empty() and submitted_drag_views.has(request_id):
        is_submitting = false


func _hand_for_seat(seat: String) -> Control:
    match seat:
        "Red":
            return red_hand
        "Blue":
            return blue_hand
        _:
            return null


func _flip_sign_for_owner(owner: String) -> float:
    return -1.0 if owner == "Blue" else 1.0


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
