extends Control

const MAIN_MENU_SCENE := "res://scenes/MainMenuScene.tscn"

var title_label: Label
var status_label: Label
var start_button: Button
var rule_buttons: Dictionary = {}
var seat_name_labels: Dictionary = {}
var seat_kind_labels: Dictionary = {}
var seat_ready_labels: Dictionary = {}
var seat_take_buttons: Dictionary = {}
var applying_snapshot := false


func _ready() -> void:
    _build_scene()
    GameFlowBridge.lobby_snapshot_changed.connect(_on_lobby_snapshot_changed)
    GameFlowBridge.match_start_failed.connect(_on_match_start_failed)

    var snapshot: Dictionary = GameFlowBridge.get_lobby_snapshot()
    if snapshot.is_empty() or not bool(snapshot.get("has_lobby", false)):
        get_tree().change_scene_to_file(MAIN_MENU_SCENE)
        return

    _apply_snapshot(snapshot)


func _build_scene() -> void:
    var background := ColorRect.new()
    background.name = "Background"
    background.color = TriadCardPalette.background()
    background.set_anchors_preset(Control.PRESET_FULL_RECT)
    background.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(background)

    var margin := MarginContainer.new()
    margin.name = "Margin"
    margin.set_anchors_preset(Control.PRESET_FULL_RECT)
    margin.add_theme_constant_override("margin_left", 48)
    margin.add_theme_constant_override("margin_top", 36)
    margin.add_theme_constant_override("margin_right", 48)
    margin.add_theme_constant_override("margin_bottom", 36)
    add_child(margin)

    var root := VBoxContainer.new()
    root.name = "LobbyRoot"
    root.add_theme_constant_override("separation", 22)
    margin.add_child(root)

    var header := HBoxContainer.new()
    header.add_theme_constant_override("separation", 16)
    root.add_child(header)

    var back_button := Button.new()
    back_button.text = "BACK"
    back_button.custom_minimum_size = Vector2(120, 44)
    back_button.pressed.connect(_on_back_pressed)
    header.add_child(back_button)

    title_label = Label.new()
    title_label.text = "LOBBY"
    title_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    title_label.add_theme_font_size_override("font_size", 34)
    title_label.add_theme_color_override("font_color", TriadCardPalette.text())
    header.add_child(title_label)

    status_label = Label.new()
    status_label.custom_minimum_size = Vector2(220, 44)
    status_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
    status_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    status_label.add_theme_font_size_override("font_size", 18)
    status_label.add_theme_color_override("font_color", TriadCardPalette.text())
    header.add_child(status_label)

    var body := HBoxContainer.new()
    body.size_flags_vertical = Control.SIZE_EXPAND_FILL
    body.add_theme_constant_override("separation", 28)
    root.add_child(body)

    var seats := HBoxContainer.new()
    seats.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    seats.add_theme_constant_override("separation", 18)
    body.add_child(seats)
    seats.add_child(_create_seat_panel("Blue"))
    seats.add_child(_create_seat_panel("Red"))

    var rules_panel := PanelContainer.new()
    rules_panel.custom_minimum_size = Vector2(320, 0)
    body.add_child(rules_panel)

    var rules_box := VBoxContainer.new()
    rules_box.add_theme_constant_override("separation", 8)
    rules_panel.add_child(rules_box)

    var rules_title := Label.new()
    rules_title.text = "RULES"
    rules_title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    rules_title.add_theme_font_size_override("font_size", 24)
    rules_title.add_theme_color_override("font_color", TriadCardPalette.text())
    rules_box.add_child(rules_title)

    for option in GameFlowBridge.get_rule_options():
        var rule_name := str(option.get("name", ""))
        var toggle := CheckButton.new()
        toggle.text = rule_name.to_upper()
        toggle.button_pressed = bool(option.get("enabled", false))
        toggle.toggled.connect(func(enabled: bool) -> void:
            _on_rule_toggled(rule_name, enabled)
        )
        rule_buttons[rule_name] = toggle
        rules_box.add_child(toggle)

    start_button = Button.new()
    start_button.text = "START GAME"
    start_button.custom_minimum_size = Vector2(260, 56)
    start_button.pressed.connect(_on_start_pressed)
    root.add_child(start_button)


func _create_seat_panel(seat: String) -> PanelContainer:
    var panel := PanelContainer.new()
    panel.custom_minimum_size = Vector2(300, 260)
    panel.size_flags_horizontal = Control.SIZE_EXPAND_FILL

    var box := VBoxContainer.new()
    box.alignment = BoxContainer.ALIGNMENT_CENTER
    box.add_theme_constant_override("separation", 12)
    panel.add_child(box)

    var seat_label := Label.new()
    seat_label.text = seat.to_upper()
    seat_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    seat_label.add_theme_font_size_override("font_size", 30)
    seat_label.add_theme_color_override("font_color", TriadCardPalette.for_seat(seat))
    box.add_child(seat_label)

    var name_label := Label.new()
    name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    name_label.add_theme_font_size_override("font_size", 24)
    name_label.add_theme_color_override("font_color", TriadCardPalette.text())
    seat_name_labels[seat] = name_label
    box.add_child(name_label)

    var kind_label := Label.new()
    kind_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    kind_label.add_theme_font_size_override("font_size", 18)
    kind_label.add_theme_color_override("font_color", TriadCardPalette.text())
    seat_kind_labels[seat] = kind_label
    box.add_child(kind_label)

    var ready_label := Label.new()
    ready_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    ready_label.add_theme_font_size_override("font_size", 16)
    ready_label.add_theme_color_override("font_color", TriadCardPalette.text())
    seat_ready_labels[seat] = ready_label
    box.add_child(ready_label)

    var take_button := Button.new()
    take_button.text = "TAKE"
    take_button.custom_minimum_size = Vector2(160, 44)
    take_button.pressed.connect(func() -> void:
        GameFlowBridge.take_seat(seat)
    )
    seat_take_buttons[seat] = take_button
    box.add_child(take_button)

    return panel


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

    applying_snapshot = false


func _on_lobby_snapshot_changed(snapshot: Dictionary) -> void:
    _apply_snapshot(snapshot)


func _on_match_start_failed(reason: String) -> void:
    status_label.text = reason.to_upper()


func _on_rule_toggled(rule_name: String, enabled: bool) -> void:
    if applying_snapshot:
        return

    GameFlowBridge.set_rule_enabled(rule_name, enabled)


func _on_start_pressed() -> void:
    GameFlowBridge.start_match()


func _on_back_pressed() -> void:
    get_tree().change_scene_to_file(MAIN_MENU_SCENE)
