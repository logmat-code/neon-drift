import os
import struct
import zlib

def make_png(width, height, rgba_data):
    def chunk(tag, data):
        return struct.pack('>I', len(data)) + tag + data + struct.pack('>I', zlib.crc32(tag + data) & 0xffffffff)
    header = b'\x89PNG\r\n\x1a\n'
    ihdr = chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0))
    raw_lines = bytearray()
    for y in range(height):
        raw_lines.append(0) # filter byte none
        for x in range(width):
            idx = (y * width + x) * 4
            raw_lines.extend(rgba_data[idx:idx+4])
    idat = chunk(b'IDAT', zlib.compress(bytes(raw_lines)))
    iend = chunk(b'IEND', b'')
    return header + ihdr + idat + iend

def create_car_sprite(path, body_col, roof_col, neon_col, glass_col=(40, 60, 90, 255), width=32, height=64):
    data = bytearray([0] * (width * height * 4))
    def set_p(x, y, col):
        if 0 <= x < width and 0 <= y < height:
            idx = (y * width + x) * 4
            data[idx:idx+4] = col

    # Car body bounds: x: 6..25 (w=20), y: 4..59 (h=56)
    for y in range(8, 56):
        # Aerodynamic profile
        dx = 1 if (y < 14 or y > 50) else 0
        for x in range(7 + dx, 25 - dx):
            set_p(x, y, body_col)

    # Front bumper (top, y: 56..60)
    for y in range(56, 60):
        for x in range(9, 23):
            set_p(x, y, body_col)

    # Rear bumper (bottom, y: 4..8)
    for y in range(4, 8):
        for x in range(8, 24):
            set_p(x, y, body_col)

    # Windshield / glass
    for y in range(36, 46):
        for x in range(10, 22):
            set_p(x, y, glass_col)

    # Rear window
    for y in range(16, 24):
        for x in range(10, 22):
            set_p(x, y, glass_col)

    # Roof
    for y in range(24, 36):
        for x in range(9, 23):
            set_p(x, y, roof_col)

    # Headlights (front y=59)
    set_p(9, 59, (255, 255, 200, 255))
    set_p(10, 59, (255, 255, 200, 255))
    set_p(21, 59, (255, 255, 200, 255))
    set_p(22, 59, (255, 255, 200, 255))

    # Taillights (rear y=4)
    for x in range(8, 12):
        set_p(x, 4, (255, 20, 40, 255))
    for x in range(20, 24):
        set_p(x, 4, (255, 20, 40, 255))

    # Wheels (tires on sides)
    for wx in (5, 6, 25, 26):
        for wy in list(range(12, 20)) + list(range(44, 52)):
            set_p(wx, wy, (20, 20, 25, 255))

    # Neon accents / stripes
    for y in range(12, 52):
        set_p(15, y, neon_col)
        set_p(16, y, neon_col)

    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'wb') as f:
        f.write(make_png(width, height, data))
    print(f"Generated car: {path}")

# Generate 5 additional unique playable cars
vehicles_dir = "/Users/logmat/Desktop/CLAUDE/NeonDrift/Assets/_Project/Art/Vehicles"
# AKUMA 86: Agile White/Black Panda Trueno AE86 style with crimson accents
create_car_sprite(os.path.join(vehicles_dir, "car_akuma_86.png"), (235, 235, 240, 255), (30, 30, 35, 255), (255, 30, 60, 255))
# RYUJIN RX: Drift master vibrant Sunburst Orange with Gold neon & dark charcoal roof
create_car_sprite(os.path.join(vehicles_dir, "car_ryujin_rx.png"), (255, 120, 20, 255), (40, 40, 45, 255), (255, 215, 0, 255))
# KAGE R34: Balanced Midnight Bayside Blue with Electric Cyan neon
create_car_sprite(os.path.join(vehicles_dir, "car_kage_r34.png"), (25, 50, 160, 255), (15, 30, 80, 255), (0, 240, 255, 255))
# RAIDEN GT: High-power Boost Acid Cyber Green with Neon Yellow neon
create_car_sprite(os.path.join(vehicles_dir, "car_raiden_gt.png"), (0, 220, 110, 255), (20, 35, 30, 255), (255, 245, 30, 255))
# ONI X: Supreme speed Dark Violet / Magenta Demon Hypercar
create_car_sprite(os.path.join(vehicles_dir, "car_oni_x.png"), (130, 20, 160, 255), (45, 10, 60, 255), (255, 0, 140, 255))

# Generate extra traffic vehicle sprites: Taxi, Sports, Pickup, Bus
traffic_dir = "/Users/logmat/Desktop/CLAUDE/NeonDrift/Assets/_Project/Art/Traffic"
# Taxi: Yellow with checkered roof
create_car_sprite(os.path.join(traffic_dir, "traffic_taxi.png"), (245, 200, 10, 255), (255, 255, 255, 255), (20, 20, 20, 255))
# Sports: Sleek Red
create_car_sprite(os.path.join(traffic_dir, "traffic_sports.png"), (220, 30, 40, 255), (30, 30, 30, 255), (255, 100, 100, 255))
# Pickup: Dark Teal, open bed
create_car_sprite(os.path.join(traffic_dir, "traffic_pickup.png"), (30, 110, 120, 255), (20, 50, 60, 255), (40, 80, 90, 255))
# Bus: Long width=36, height=96 Navy Blue Transit Bus
bus_data = bytearray([0] * (36 * 96 * 4))
for y in range(4, 92):
    for x in range(6, 30):
        idx = (y * 36 + x) * 4
        # Windows on sides
        if 14 <= y <= 82 and (x in (7, 8, 27, 28) or y in range(14, 82, 10)):
            bus_data[idx:idx+4] = bytes([80, 160, 220, 255])
        else:
            bus_data[idx:idx+4] = bytes([25, 70, 160, 255])
with open(os.path.join(traffic_dir, "traffic_bus.png"), 'wb') as f:
    f.write(make_png(36, 96, bus_data))
print("Generated traffic_bus.png")

# Generate App Icon 512x512
icon_dir = "/Users/logmat/Desktop/CLAUDE/NeonDrift/Assets/_Project/Art"
icon_data = bytearray([0] * (512 * 512 * 4))
for y in range(512):
    for x in range(512):
        idx = (y * 512 + x) * 4
        # Radial / dark city backdrop gradient
        dy = y / 512.0
        r = int(12 + 20 * (1.0 - dy))
        g = int(14 + 10 * (1.0 - dy))
        b = int(25 + 40 * dy)
        icon_data[idx:idx+4] = bytes([r, g, b, 255])

# Draw stylised angled road ribbon & neon lines in icon center
for y in range(512):
    cx = int(256 + (y - 256) * 0.25)
    for x in range(cx - 120, cx + 120):
        if 0 <= x < 512:
            idx = (y * 512 + x) * 4
            icon_data[idx:idx+4] = bytes([28, 30, 38, 255])
    # Cyan curb left
    for x in range(cx - 125, cx - 120):
        if 0 <= x < 512:
            idx = (y * 512 + x) * 4
            icon_data[idx:idx+4] = bytes([0, 240, 255, 255])
    # Magenta curb right
    for x in range(cx + 120, cx + 125):
        if 0 <= x < 512:
            idx = (y * 512 + x) * 4
            icon_data[idx:idx+4] = bytes([255, 0, 140, 255])
    # Gold dashed centerline
    if (y // 32) % 2 == 0:
        for x in range(cx - 4, cx + 4):
            if 0 <= x < 512:
                idx = (y * 512 + x) * 4
                icon_data[idx:idx+4] = bytes([255, 215, 0, 255])

# Draw KITSUNE S13 centered in icon (scaled up 5x)
car_cx, car_cy = 256, 230
for cy in range(64):
    for cx in range(32):
        target_x = car_cx + (cx - 16) * 5
        target_y = car_cy + (cy - 32) * 5
        # Draw car pixels if inside bounds
        if 7 <= cx <= 24 and 6 <= cy <= 58:
            col = (255, 255, 255, 255) # white Kitsune body
            if 36 <= cy <= 46 or 16 <= cy <= 24:
                col = (40, 60, 90, 255) # glass
            elif 24 <= cy <= 36:
                col = (255, 0, 128, 255) # magenta roof
            elif cy == 58:
                col = (255, 255, 180, 255) # lights
            for oy in range(5):
                for ox in range(5):
                    px, py = target_x + ox, target_y + oy
                    if 0 <= px < 512 and 0 <= py < 512:
                        pidx = (py * 512 + px) * 4
                        icon_data[pidx:pidx+4] = bytes(col)

with open(os.path.join(icon_dir, "AppIcon_512.png"), 'wb') as f:
    f.write(make_png(512, 512, icon_data))
print("Generated AppIcon_512.png")
