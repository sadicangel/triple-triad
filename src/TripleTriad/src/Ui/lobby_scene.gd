extends Control

const MAIN_MENU_SCENE := "res://scenes/MainMenuScene.tscn"

@onready var background: ColorRect = $Background
@onready var title_label: Label = $Margin/LobbyRoot/Header/TitleLabel
@onready var status_label: Label = $Margin/LobbyRoot/Header/StatusLabel
@onready var start_button: Button = $Margin/LobbyRoot/StartButton
@onready var back_button: Button = $Margin/LobbyRoot/Header/BackButton
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


func _ready() -> void:
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


func _on_rule_toggled(enabled: bool, rule_name: String) -> void:
    if applying_snapshot:
        return

    GameFlowBridge.set_rule_enabled(rule_name, enabled)


func _on_start_pressed() -> void:
    GameFlowBridge.start_match()


func _on_back_pressed() -> void:
    get_tree().change_scene_to_file(MAIN_MENU_SCENE)
