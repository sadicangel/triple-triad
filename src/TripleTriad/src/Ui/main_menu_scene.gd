extends Control

const LOBBY_SCENE := "res://scenes/LobbyScene.tscn"

var menu_root: VBoxContainer


func _ready() -> void:
    _build_scene()


func _build_scene() -> void:
    var background := ColorRect.new()
    background.name = "Background"
    background.color = TriadCardPalette.background()
    background.set_anchors_preset(Control.PRESET_FULL_RECT)
    background.mouse_filter = Control.MOUSE_FILTER_IGNORE
    add_child(background)

    var center := CenterContainer.new()
    center.name = "Center"
    center.set_anchors_preset(Control.PRESET_FULL_RECT)
    add_child(center)

    menu_root = VBoxContainer.new()
    menu_root.name = "Menu"
    menu_root.custom_minimum_size = Vector2(360, 360)
    menu_root.alignment = BoxContainer.ALIGNMENT_CENTER
    menu_root.add_theme_constant_override("separation", 14)
    center.add_child(menu_root)

    var title := Label.new()
    title.text = "TRIPLE TRIAD"
    title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    title.add_theme_font_size_override("font_size", 42)
    title.add_theme_color_override("font_color", TriadCardPalette.text())
    menu_root.add_child(title)

    menu_root.add_child(_create_button("SOLO", _on_solo_pressed))
    menu_root.add_child(_create_button("HOST", _on_host_pressed))
    menu_root.add_child(_create_button("JOIN", Callable(), true))
    menu_root.add_child(_create_button("SETTINGS", Callable(), true))


func _create_button(text: String, pressed_callback: Callable, disabled: bool = false) -> Button:
    var button := Button.new()
    button.text = text
    button.disabled = disabled
    button.custom_minimum_size = Vector2(320, 52)
    button.add_theme_font_size_override("font_size", 22)
    if not disabled:
        button.pressed.connect(pressed_callback)
    return button


func _on_solo_pressed() -> void:
    GameFlowBridge.start_solo_lobby()
    get_tree().change_scene_to_file(LOBBY_SCENE)


func _on_host_pressed() -> void:
    GameFlowBridge.start_host_lobby()
    get_tree().change_scene_to_file(LOBBY_SCENE)
