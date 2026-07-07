extends Control

const LOBBY_SCENE := "res://scenes/LobbyScene.tscn"

@onready var background: ColorRect = $Background
@onready var title_label: Label = $Center/Menu/Title
@onready var solo_button: Button = $Center/Menu/SoloButton
@onready var host_button: Button = $Center/Menu/HostButton
@onready var join_button: Button = $Center/Menu/JoinButton
@onready var settings_button: Button = $Center/Menu/SettingsButton


func _ready() -> void:
	background.color = TriadCardPalette.background()
	title_label.add_theme_color_override("font_color", TriadCardPalette.text())
	solo_button.pressed.connect(_on_solo_pressed)
	host_button.pressed.connect(_on_host_pressed)
	join_button.disabled = true
	settings_button.disabled = true


func _on_solo_pressed() -> void:
	GameFlowBridge.start_solo_lobby()
	get_tree().change_scene_to_file(LOBBY_SCENE)


func _on_host_pressed() -> void:
	GameFlowBridge.start_host_lobby()
	get_tree().change_scene_to_file(LOBBY_SCENE)
