extends Control
class_name TriadCardView

signal drag_requested(card: Dictionary, source_view: Control)

const CARD_SIZE := Vector2(256, 256)

var atlas: Texture2D
var card: Dictionary = {}
var hover_lift_enabled := false
var is_drag_preview := false
var is_hovered := false
var layout_position := Vector2.ZERO
var normal_z_index := 0
var hover_z_index := 100
var hover_tween: Tween


func _ready() -> void:
    custom_minimum_size = CARD_SIZE
    size = CARD_SIZE
    pivot_offset = CARD_SIZE * 0.5
    mouse_filter = Control.MOUSE_FILTER_IGNORE
    mouse_entered.connect(_on_mouse_entered)
    mouse_exited.connect(_on_mouse_exited)


func setup(new_atlas: Texture2D) -> void:
    atlas = new_atlas
    queue_redraw()


func bind(card_data: Dictionary, enable_hover_lift: bool) -> void:
    card = card_data.duplicate(true)
    hover_lift_enabled = enable_hover_lift
    mouse_filter = Control.MOUSE_FILTER_STOP if not is_drag_preview and bool(card.get("playable", false)) else Control.MOUSE_FILTER_IGNORE
    tooltip_text = _tooltip_text()
    queue_redraw()


func set_layout_position(new_position: Vector2, new_normal_z_index: int) -> void:
    layout_position = new_position
    normal_z_index = new_normal_z_index
    hover_z_index = normal_z_index + 100
    position = layout_position
    z_index = normal_z_index


func set_drag_visual_preview(enabled: bool) -> void:
    is_drag_preview = enabled
    mouse_filter = Control.MOUSE_FILTER_IGNORE
    modulate = Color(1, 1, 1, 0.92) if enabled else Color.WHITE
    queue_redraw()


func _gui_input(event: InputEvent) -> void:
    if card.is_empty() or not bool(card.get("playable", false)) or is_drag_preview:
        return

    if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
        accept_event()
        drag_requested.emit(card, self)


func _draw() -> void:
    if atlas == null or card.is_empty():
        return

    if not bool(card.get("face_up", false)):
        _draw_tile(TriadAtlasRegions.fill(0), Color.WHITE)
        return

    _draw_tile(TriadAtlasRegions.fill(1), TriadCardPalette.for_seat(str(card.get("owner", "Blue"))))
    _draw_tile(TriadAtlasRegions.card_face(int(card.get("number", 1))), Color.WHITE)
    _draw_tile(TriadAtlasRegions.value("west", int(card.get("w", 0))), Color.WHITE)
    _draw_tile(TriadAtlasRegions.value("north", int(card.get("n", 0))), Color.WHITE)
    _draw_tile(TriadAtlasRegions.value("east", int(card.get("e", 0))), Color.WHITE)
    _draw_tile(TriadAtlasRegions.value("south", int(card.get("s", 0))), Color.WHITE)

    var element_name := str(card.get("element", "None"))
    if element_name != "None":
        _draw_tile(TriadAtlasRegions.element(element_name), Color.WHITE)

    if is_hovered and bool(card.get("playable", false)):
        _draw_tile(TriadAtlasRegions.fill(1), Color(1, 1, 1, 0.18))


func _draw_tile(region: Rect2, color: Color) -> void:
    draw_texture_rect_region(atlas, Rect2(Vector2.ZERO, size), region, color)


func _on_mouse_entered() -> void:
    if not bool(card.get("playable", false)) or not hover_lift_enabled or is_drag_preview:
        return

    is_hovered = true
    z_index = hover_z_index
    _animate_hover(layout_position + Vector2(0, -18), Vector2(1.04, 1.04))
    queue_redraw()


func _on_mouse_exited() -> void:
    if not is_hovered:
        return

    is_hovered = false
    z_index = normal_z_index
    _animate_hover(layout_position, Vector2.ONE)
    queue_redraw()


func _animate_hover(target_position: Vector2, target_scale: Vector2) -> void:
    if hover_tween != null:
        hover_tween.kill()

    hover_tween = create_tween()
    hover_tween.set_parallel(true)
    hover_tween.tween_property(self, "position", target_position, 0.08).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    hover_tween.tween_property(self, "scale", target_scale, 0.08).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)


func _tooltip_text() -> String:
    if card.is_empty():
        return ""
    if not bool(card.get("face_up", false)):
        return "Hidden card"

    return "%s  W %s  N %s  E %s  S %s" % [
        str(card.get("name", "")),
        _rank(int(card.get("w", 0))),
        _rank(int(card.get("n", 0))),
        _rank(int(card.get("e", 0))),
        _rank(int(card.get("s", 0))),
    ]


func _rank(rank: int) -> String:
    return "A" if rank == 10 else str(rank)
