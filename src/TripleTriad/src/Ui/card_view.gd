extends Control
class_name TriadCardView

signal drag_requested(card: Dictionary, source_view: Control)

const CARD_SIZE := Vector2(256, 256)

var atlas: Texture2D
var card: Dictionary = {}
var hover_lift_enabled := false
var is_drag_preview := false
var is_drag_placeholder := false
var is_hovered := false
var layout_position := Vector2.ZERO
var normal_z_index := 0
var hover_z_index := 100
var hover_tween: Tween
var display_size := CARD_SIZE


func _ready() -> void:
    _apply_display_size()
    mouse_filter = Control.MOUSE_FILTER_IGNORE
    mouse_entered.connect(_on_mouse_entered)
    mouse_exited.connect(_on_mouse_exited)


func set_display_size(new_size: Vector2) -> void:
    display_size = new_size
    _apply_display_size()
    queue_redraw()


func setup(new_atlas: Texture2D) -> void:
    atlas = new_atlas
    queue_redraw()


func bind(card_data: Dictionary, enable_hover_lift: bool) -> void:
    card = card_data.duplicate(true)
    hover_lift_enabled = enable_hover_lift
    _update_mouse_filter()
    tooltip_text = _tooltip_text()
    queue_redraw()


func set_card_owner(owner: String) -> void:
    if card.is_empty():
        return

    card["owner"] = owner
    card["face_up"] = true
    queue_redraw()


func set_layout_position(new_position: Vector2, new_normal_z_index: int) -> void:
    layout_position = new_position
    normal_z_index = new_normal_z_index
    hover_z_index = normal_z_index + 100
    position = layout_position
    z_index = normal_z_index


func set_drag_visual_preview(enabled: bool) -> void:
    is_drag_preview = enabled
    _update_mouse_filter()
    modulate = Color(1, 1, 1, 0.92) if enabled else Color.WHITE
    queue_redraw()


func set_drag_placeholder(enabled: bool) -> void:
    is_drag_placeholder = enabled
    if enabled:
        _reset_hover_state()

    modulate = Color.WHITE
    _update_mouse_filter()
    queue_redraw()


func animate_owner_flip(previous_owner: String, new_owner: String, flip_sign: float) -> void:
    if card.is_empty():
        return

    set_card_owner(previous_owner)
    var start_position := position
    var start_scale := Vector2.ONE
    scale = start_scale
    pivot_offset = CARD_SIZE * 0.5

    var lift_position := start_position + Vector2(10.0 * flip_sign, -20.0)
    var first_half := create_tween()
    first_half.set_parallel(true)
    first_half.tween_property(self, "position", lift_position, 0.12).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    first_half.tween_property(self, "scale", Vector2(0.0, 1.08), 0.12).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
    await first_half.finished

    set_card_owner(new_owner)

    var second_half := create_tween()
    second_half.set_parallel(true)
    second_half.tween_property(self, "position", start_position + Vector2(-8.0 * flip_sign, -16.0), 0.12).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    second_half.tween_property(self, "scale", Vector2(1.0, 1.06), 0.12).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    await second_half.finished

    var third_half := create_tween()
    third_half.set_parallel(true)
    third_half.tween_property(self, "position", start_position + Vector2(-10.0 * flip_sign, -18.0), 0.10).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
    third_half.tween_property(self, "scale", Vector2(0.0, 1.08), 0.10).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
    await third_half.finished

    var settle := create_tween()
    settle.set_parallel(true)
    settle.tween_property(self, "position", start_position, 0.12).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    settle.tween_property(self, "scale", start_scale, 0.12).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
    await settle.finished


func _gui_input(event: InputEvent) -> void:
    if card.is_empty() or not bool(card.get("playable", false)) or is_drag_preview or is_drag_placeholder:
        return

    if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
        accept_event()
        drag_requested.emit(card, self)


func _draw() -> void:
    if atlas == null or card.is_empty():
        return

    if is_drag_placeholder:
        _draw_placeholder()
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


func _draw_placeholder() -> void:
    var owner_color := TriadCardPalette.for_seat(str(card.get("owner", "Blue")))
    var fill_color := owner_color.darkened(0.35)
    fill_color.a = 0.18
    var border_color := owner_color
    border_color.a = 0.52
    var guide_color := owner_color
    guide_color.a = 0.26
    var rect := Rect2(Vector2(12, 12), size - Vector2(24, 24))
    draw_rect(Rect2(Vector2.ZERO, size), fill_color, true)
    draw_rect(rect, border_color, false, 4.0)
    draw_line(rect.position + Vector2(18, 18), rect.end - Vector2(18, 18), guide_color, 3.0)
    draw_line(Vector2(rect.end.x - 18, rect.position.y + 18), Vector2(rect.position.x + 18, rect.end.y - 18), guide_color, 3.0)


func _on_mouse_entered() -> void:
    if not bool(card.get("playable", false)) or not hover_lift_enabled or is_drag_preview or is_drag_placeholder:
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


func _reset_hover_state() -> void:
    if hover_tween != null:
        hover_tween.kill()

    is_hovered = false
    position = layout_position
    scale = Vector2.ONE
    z_index = normal_z_index


func _update_mouse_filter() -> void:
    mouse_filter = Control.MOUSE_FILTER_STOP if not is_drag_preview and not is_drag_placeholder and bool(card.get("playable", false)) else Control.MOUSE_FILTER_IGNORE


func _apply_display_size() -> void:
    custom_minimum_size = display_size
    size = display_size
    pivot_offset = display_size * 0.5


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
