extends Control

const LOBBY_SCENE := "res://scenes/LobbyScene.tscn"
const CardViewScene := preload("res://scenes/CardView.tscn")
const MAX_SELECTION := 5
const CELL_SIZE := Vector2(104, 116)
const CARD_DRAW_SIZE := Vector2(88, 88)
const FULL_PREVIEW_MARGIN := 12.0

var atlas: Texture2D
var selected_numbers: Array[int] = []
var card_buttons: Dictionary = {}
var counter_label: Label
var confirm_button: Button
var grid: GridContainer
var full_preview: Control


func _ready() -> void:
    atlas = load("res://assets/triple_triad/spritesheet.png")

    var snapshot: Dictionary = GameFlowBridge.get_lobby_snapshot()
    if snapshot.is_empty() or not bool(snapshot.get("can_select_cards", false)):
        get_tree().call_deferred("change_scene_to_file", LOBBY_SCENE)
        return

    _read_initial_selection(snapshot)
    _build_scene()
    _populate_cards()
    _refresh_selection_state()


func _read_initial_selection(snapshot: Dictionary) -> void:
    selected_numbers.clear()
    var selected_cards: Array = snapshot.get("selected_cards", [])
    for card in selected_cards:
        var card_data: Dictionary = card
        var number := int(card_data.get("number", 0))
        if number > 0 and not selected_numbers.has(number):
            selected_numbers.append(number)


func _build_scene() -> void:
    var background := ColorRect.new()
    background.color = TriadCardPalette.background()
    background.mouse_filter = Control.MOUSE_FILTER_IGNORE
    background.set_anchors_preset(Control.PRESET_FULL_RECT)
    add_child(background)

    var margin := MarginContainer.new()
    margin.set_anchors_preset(Control.PRESET_FULL_RECT)
    margin.add_theme_constant_override("margin_left", 48)
    margin.add_theme_constant_override("margin_top", 36)
    margin.add_theme_constant_override("margin_right", 48)
    margin.add_theme_constant_override("margin_bottom", 36)
    add_child(margin)

    var root := VBoxContainer.new()
    root.add_theme_constant_override("separation", 18)
    margin.add_child(root)

    var header := HBoxContainer.new()
    header.add_theme_constant_override("separation", 16)
    root.add_child(header)

    var cancel_button := Button.new()
    cancel_button.text = "CANCEL"
    cancel_button.custom_minimum_size = Vector2(130, 44)
    cancel_button.pressed.connect(_on_cancel_pressed)
    header.add_child(cancel_button)

    var title := Label.new()
    title.text = "SELECT HAND"
    title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    title.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    title.add_theme_font_size_override("font_size", 34)
    title.add_theme_color_override("font_color", TriadCardPalette.text())
    header.add_child(title)

    counter_label = Label.new()
    counter_label.custom_minimum_size = Vector2(100, 44)
    counter_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
    counter_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    counter_label.add_theme_font_size_override("font_size", 20)
    counter_label.add_theme_color_override("font_color", TriadCardPalette.text())
    header.add_child(counter_label)

    var randomize_button := Button.new()
    randomize_button.text = "RANDOMIZE"
    randomize_button.custom_minimum_size = Vector2(150, 44)
    randomize_button.pressed.connect(_on_randomize_pressed)
    header.add_child(randomize_button)

    confirm_button = Button.new()
    confirm_button.text = "CONFIRM"
    confirm_button.custom_minimum_size = Vector2(140, 44)
    confirm_button.pressed.connect(_on_confirm_pressed)
    header.add_child(confirm_button)

    var scroll := ScrollContainer.new()
    scroll.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
    root.add_child(scroll)

    grid = GridContainer.new()
    grid.columns = 10
    grid.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
    grid.add_theme_constant_override("h_separation", 10)
    grid.add_theme_constant_override("v_separation", 10)
    scroll.add_child(grid)


func _populate_cards() -> void:
    card_buttons.clear()
    var catalog: Array = GameFlowBridge.get_lobby_card_catalog()
    for card in catalog:
        var card_data: Dictionary = card
        var number := int(card_data.get("number", 0))
        if number <= 0:
            continue

        var button := Button.new()
        button.custom_minimum_size = CELL_SIZE
        button.focus_mode = Control.FOCUS_NONE
        button.clip_contents = true
        button.tooltip_text = str(card_data.get("name", ""))
        button.pressed.connect(_on_card_pressed.bind(number))
        button.mouse_entered.connect(_show_full_preview.bind(card_data, button))
        button.mouse_exited.connect(_hide_full_preview)
        grid.add_child(button)

        var view := CardViewScene.instantiate()
        view.position = Vector2((CELL_SIZE.x - CARD_DRAW_SIZE.x) * 0.5, 8)
        button.add_child(view)
        view.set_display_size(CARD_DRAW_SIZE)
        view.setup(atlas)
        view.bind(card_data, false)

        card_buttons[number] = button


func _on_card_pressed(number: int) -> void:
    if selected_numbers.has(number):
        selected_numbers.erase(number)
    elif selected_numbers.size() < MAX_SELECTION:
        selected_numbers.append(number)

    _refresh_selection_state()


func _on_randomize_pressed() -> void:
    var catalog: Array = GameFlowBridge.get_lobby_card_catalog()
    catalog.shuffle()
    selected_numbers.clear()

    for i in min(MAX_SELECTION, catalog.size()):
        var card_data: Dictionary = catalog[i]
        selected_numbers.append(int(card_data.get("number", 0)))

    _refresh_selection_state()


func _on_confirm_pressed() -> void:
    if selected_numbers.size() != MAX_SELECTION:
        return

    _hide_full_preview()
    var card_numbers: Array = []
    for number in selected_numbers:
        card_numbers.append(number)

    GameFlowBridge.set_lobby_card_selection(card_numbers)
    get_tree().change_scene_to_file(LOBBY_SCENE)


func _on_cancel_pressed() -> void:
    _hide_full_preview()
    get_tree().change_scene_to_file(LOBBY_SCENE)


func _refresh_selection_state() -> void:
    counter_label.text = "%s / %s" % [selected_numbers.size(), MAX_SELECTION]
    confirm_button.disabled = selected_numbers.size() != MAX_SELECTION

    for number in card_buttons.keys():
        var button: Button = card_buttons[number]
        _apply_card_style(button, selected_numbers.has(number))


func _apply_card_style(button: Button, selected: bool) -> void:
    button.add_theme_stylebox_override("normal", _card_style(selected, false))
    button.add_theme_stylebox_override("hover", _card_style(selected, true))
    button.add_theme_stylebox_override("pressed", _card_style(true, true))


func _card_style(selected: bool, hover: bool) -> StyleBoxFlat:
    var style := StyleBoxFlat.new()
    style.bg_color = Color(0.12, 0.22, 0.19) if selected else Color(0.055, 0.065, 0.075)
    if hover:
        style.bg_color = style.bg_color.lightened(0.08)

    style.border_color = TriadCardPalette.drop_preview_border() if selected else Color(0.20, 0.24, 0.27)
    style.border_width_left = 3
    style.border_width_top = 3
    style.border_width_right = 3
    style.border_width_bottom = 3
    style.corner_radius_top_left = 4
    style.corner_radius_top_right = 4
    style.corner_radius_bottom_right = 4
    style.corner_radius_bottom_left = 4
    return style


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
