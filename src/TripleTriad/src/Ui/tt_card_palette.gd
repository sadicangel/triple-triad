extends RefCounted
class_name TriadCardPalette

static func background() -> Color:
    return Color(0.025, 0.032, 0.045)


static func board_panel() -> Color:
    return Color(0.08, 0.10, 0.11, 0.95)


static func board_slot() -> Color:
    return Color(0.77, 0.68, 0.46, 0.22)


static func board_slot_border() -> Color:
    return Color(0.94, 0.84, 0.54, 0.65)


static func drop_preview() -> Color:
    return Color(0.44, 0.88, 0.76, 0.42)


static func drop_preview_border() -> Color:
    return Color(0.62, 1.0, 0.88, 0.95)


static func text() -> Color:
    return Color(0.92, 0.94, 0.92)


static func for_seat(seat: String) -> Color:
    match seat:
        "Red":
            return Color(0.82, 0.18, 0.30)
        "Blue":
            return Color(0.18, 0.38, 0.86)
        _:
            return Color.WHITE
