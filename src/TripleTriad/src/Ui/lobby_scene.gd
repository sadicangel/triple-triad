extends Control

const MAIN_MENU_SCENE := "res://scenes/MainMenuScene.tscn"
const HAND_SELECTOR_SCENE := "res://scenes/HandSelectorScene.tscn"
const CardViewScene := preload("res://scenes/CardView.tscn")
const PREVIEW_CARD_SIZE := Vector2(56, 56)
const FULL_PREVIEW_MARGIN := 12.0

@onready var background: ColorRect = $Background
@onready var title_label: Label = $Margin/LobbyRoot/Header/TitleLabel
@onready var status_label: Label = $Margin/LobbyRoot/Header/StatusLabel
@onready var start_button: Button = $Margin/LobbyRoot/StartButton
@onready var back_button: Button = $Margin/LobbyRoot/Header/BackButton
@onready var rules_box: VBoxContainer = $Margin/LobbyRoot/Body/RulesPanel/RulesBox
@onready var rules_list: VBoxContainer = $Margin/LobbyRoot/Body/RulesPanel/RulesBox/RulesList
@onready var seat_name_labels := {
    "Blue": $Margin/LobbyRoot/Body/Seats/BlueSeat/Box/NameLabel,
    "Red": $Margin/LobbyRoot/Body/Seats/RedSeat/Box/NameLabel,
}
@onready var seat_kind_labels := {
    "Blue": $Margin/LobbyRoot/Body/Seats/BlueSeat/Box/KindLabel,
    "Red": $Margin/LobbyRoot/Body/Seats/RedSeat/Box/KindLabel,
}
@onready var seat_ready_labels := {
    "Blue": $Margin/LobbyRoot/Body/Seats/BlueSeat/Box/ReadyLabel,
    "Red": $Margin/LobbyRoot/Body/Seats/RedSeat/Box/ReadyLabel,
}
@onready var seat_take_buttons := {
    "Blue": $Margin/LobbyRoot/Body/Seats/BlueSeat/Box/TakeButton,
    "Red": $Margin/LobbyRoot/Body/Seats/RedSeat/Box/TakeButton,
}
@onready var seat_labels := {
    "Blue": $Margin/LobbyRoot/Body/Seats/BlueSeat/Box/SeatLabel,
    "Red": $Margin/LobbyRoot/Body/Seats/RedSeat/Box/SeatLabel,
}
var rule_buttons: Dictionary = {}
var applying_snapshot := false
var atlas: Texture2D
var card_preview_row: HBoxContainer
var select_cards_button: Button
var full_preview: Control


func _ready() -> void:
    atlas = load("res://assets/triple_triad/spritesheet.png")
    background.color = TriadCardPalette.background()
    title_label.add_theme_color_override("font_color", TriadCardPalette.text())
    status_label.add_theme_color_override("font_color", TriadCardPalette.text())
    seat_labels["Blue"].add_theme_color_override("font_color", TriadCardPalette.for_seat("Blue"))
    seat_labels["Red"].add_theme_color_override("font_color", TriadCardPalette.for_seat("Red"))

    back_button.pressed.connect(_on_back_pressed)
    start_button.pressed.connect(_on_start_pressed)
    seat_take_buttons["Blue"].pressed.connect(func() -> void:
        GameFlowBridge.take_seat("Blue")
    )
    seat_take_buttons["Red"].pressed.connect(func() -> void:
        GameFlowBridge.take_seat("Red")
    )

    GameFlowBridge.lobby_snapshot_changed.connect(_on_lobby_snapshot_changed)
    GameFlowBridge.match_start_failed.connect(_on_match_start_failed)

    _build_rule_toggles()
    _build_card_selection_controls()

    var snapshot: Dictionary = GameFlowBridge.get_lobby_snapshot()
    if snapshot.is_empty() or not bool(snapshot.get("has_lobby", false)):
        get_tree().call_deferred("change_scene_to_file", MAIN_MENU_SCENE)
        return

    _apply_snapshot(snapshot)


func _build_rule_toggles() -> void:
    for child in rules_list.get_children():
        child.queue_free()

    rule_buttons.clear()
    for option in GameFlowBridge.get_rule_options():
        var rule_name := str(option.get("name", ""))
        var toggle := CheckButton.new()
        toggle.text = rule_name.to_upper()
        toggle.button_pressed = bool(option.get("enabled", false))
        toggle.toggled.connect(_on_rule_toggled.bind(rule_name))
        rule_buttons[rule_name] = toggle
        rules_list.add_child(toggle)


func _build_card_selection_controls() -> void:
    var title := Label.new()
    title.text = "YOUR HAND"
    title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    title.add_theme_font_size_override("font_size", 18)
    title.add_theme_color_override("font_color", TriadCardPalette.text())
    rules_box.add_child(title)

    card_preview_row = HBoxContainer.new()
    card_preview_row.alignment = BoxContainer.ALIGNMENT_CENTER
    card_preview_row.add_theme_constant_override("separation", 6)
    rules_box.add_child(card_preview_row)

    select_cards_button = Button.new()
    select_cards_button.text = "SELECT CARDS"
    select_cards_button.custom_minimum_size = Vector2(220, 44)
    select_cards_button.pressed.connect(_on_select_cards_pressed)
    rules_box.add_child(select_cards_button)

    _show_preview_placeholders()


func _apply_snapshot(snapshot: Dictionary) -> void:
    applying_snapshot = true

    title_label.text = "%s LOBBY" % str(snapshot.get("mode", ""))
    status_label.text = str(snapshot.get("status", "")).to_upper()
    start_button.disabled = not bool(snapshot.get("can_start", false))
    start_button.text = "STARTING" if bool(snapshot.get("is_match_starting", false)) else "START GAME"

    for seat_data in snapshot.get("seats", []):
        var seat := str(seat_data.get("seat", ""))
        if seat.is_empty():
            continue

        seat_name_labels[seat].text = str(seat_data.get("name", "OPEN")).to_upper()
        seat_kind_labels[seat].text = str(seat_data.get("kind", "Empty")).to_upper()
        seat_ready_labels[seat].text = "LOCAL" if bool(seat_data.get("is_local", false)) else (
            "READY" if bool(seat_data.get("ready", false)) else ""
        )
        seat_take_buttons[seat].visible = bool(seat_data.get("can_take", false))
        seat_take_buttons[seat].disabled = not bool(seat_data.get("can_take", false))

    for option in GameFlowBridge.get_rule_options():
        var rule_name := str(option.get("name", ""))
        if rule_buttons.has(rule_name):
            rule_buttons[rule_name].set_pressed_no_signal(bool(option.get("enabled", false)))

    _apply_card_selection(snapshot)

    applying_snapshot = false


func _apply_card_selection(snapshot: Dictionary) -> void:
    var can_select := bool(snapshot.get("can_select_cards", false))
    var is_random := _is_random_rule_enabled(snapshot)
    select_cards_button.disabled = not can_select
    card_preview_row.modulate = Color.WHITE if can_select or is_random else Color(1, 1, 1, 0.35)

    _clear_preview()
    if is_random:
        _show_preview_backs(str(snapshot.get("local_seat", "Blue")))
        return

    var selected_cards: Array = snapshot.get("selected_cards", [])
    if selected_cards.is_empty():
        _show_preview_placeholders()
        return

    for i in 5:
        if i < selected_cards.size():
            _add_preview_card(selected_cards[i])
        else:
            _add_preview_placeholder()


func _show_preview_placeholders() -> void:
    _clear_preview()
    for i in 5:
        _add_preview_placeholder()


func _show_preview_backs(owner: String) -> void:
    for i in 5:
        _add_preview_back(owner)


func _clear_preview() -> void:
    if card_preview_row == null:
        return

    _hide_full_preview()
    for child in card_preview_row.get_children():
        card_preview_row.remove_child(child)
        child.queue_free()


func _add_preview_placeholder() -> void:
    var slot := PanelContainer.new()
    slot.custom_minimum_size = PREVIEW_CARD_SIZE
    slot.clip_contents = true
    var fill := ColorRect.new()
    fill.color = TriadCardPalette.board_slot()
    fill.custom_minimum_size = PREVIEW_CARD_SIZE
    slot.add_child(fill)
    card_preview_row.add_child(slot)


func _add_preview_back(owner: String) -> void:
    var card_data := _card_back_data(owner)
    _add_preview_card(card_data)


func _add_preview_card(card: Dictionary) -> void:
    var slot := Control.new()
    slot.custom_minimum_size = PREVIEW_CARD_SIZE
    slot.clip_contents = true
    slot.mouse_filter = Control.MOUSE_FILTER_STOP
    card_preview_row.add_child(slot)

    var view := CardViewScene.instantiate()
    slot.add_child(view)
    view.set_display_size(PREVIEW_CARD_SIZE)
    view.setup(atlas)
    var card_data := card.duplicate(true)
    if not card_data.has("face_up"):
        card_data["face_up"] = true
    card_data["playable"] = false
    view.bind(card_data, false)
    slot.mouse_entered.connect(_show_full_preview.bind(card_data, slot))
    slot.mouse_exited.connect(_hide_full_preview)


func _show_full_preview(card: Dictionary, anchor: Control) -> void:
    _hide_full_preview()

    var preview := CardViewScene.instantiate()
    full_preview = preview
    full_preview.z_index = 4000
    full_preview.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(full_preview)

    preview.setup(atlas)
    var card_data := card.duplicate(true)
    card_data["playable"] = false
    preview.bind(card_data, false)
    preview.position = _full_preview_position(anchor)


func _hide_full_preview() -> void:
    if full_preview != null and is_instance_valid(full_preview):
        full_preview.queue_free()

    full_preview = null


func _full_preview_position(anchor: Control) -> Vector2:
    var anchor_rect := anchor.get_global_rect()
    var local_anchor_position: Vector2 = get_global_transform().affine_inverse() * anchor_rect.position
    var viewport_size := get_viewport_rect().size
    var preview_size := TriadCardView.CARD_SIZE
    var position := local_anchor_position + Vector2(anchor_rect.size.x + FULL_PREVIEW_MARGIN, 0)

    if position.x + preview_size.x + FULL_PREVIEW_MARGIN > viewport_size.x:
        position.x = local_anchor_position.x - preview_size.x - FULL_PREVIEW_MARGIN
    if position.y + preview_size.y + FULL_PREVIEW_MARGIN > viewport_size.y:
        position.y = viewport_size.y - preview_size.y - FULL_PREVIEW_MARGIN

    var max_x := viewport_size.x - preview_size.x - FULL_PREVIEW_MARGIN
    var max_y := viewport_size.y - preview_size.y - FULL_PREVIEW_MARGIN
    if max_x < FULL_PREVIEW_MARGIN:
        max_x = FULL_PREVIEW_MARGIN
    if max_y < FULL_PREVIEW_MARGIN:
        max_y = FULL_PREVIEW_MARGIN

    position.x = clampf(position.x, FULL_PREVIEW_MARGIN, max_x)
    position.y = clampf(position.y, FULL_PREVIEW_MARGIN, max_y)
    return position


func _is_random_rule_enabled(snapshot: Dictionary) -> bool:
    for rule in snapshot.get("rules", []):
        if str(rule) == "Random":
            return true

    return false


func _card_back_data(owner: String) -> Dictionary:
    return {
        "id": "card-back",
        "number": 1,
        "name": "Random card",
        "element": "None",
        "owner": owner,
        "face_up": false,
        "playable": false,
        "w": 0,
        "n": 0,
        "e": 0,
        "s": 0,
    }


func _on_lobby_snapshot_changed(snapshot: Dictionary) -> void:
    _apply_snapshot(snapshot)


func _on_match_start_failed(reason: String) -> void:
    status_label.text = reason.to_upper()


func _on_rule_toggled(enabled: bool, rule_name: String) -> void:
    if applying_snapshot:
        return

    GameFlowBridge.set_rule_enabled(rule_name, enabled)


func _on_start_pressed() -> void:
    GameFlowBridge.start_match()


func _on_select_cards_pressed() -> void:
    get_tree().change_scene_to_file(HAND_SELECTOR_SCENE)


func _on_back_pressed() -> void:
    get_tree().change_scene_to_file(MAIN_MENU_SCENE)
