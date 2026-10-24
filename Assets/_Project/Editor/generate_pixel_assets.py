#!/usr/bin/env python3
"""
NEON DRIFT — Master Pixel Art Generator
Generates consistent, palette-unified, top-down 3/4 pixel art sprites for Neon Drift.
PPU = 16, strictly adhering to VISUAL_STYLE_BIBLE.md.
"""

import os
from PIL import Image, ImageDraw

PROJECT_ROOT = "/Users/logmat/Desktop/CLAUDE/NeonDrift/Assets/_Project/Art"

# Master Palette
PAL = {
    "void": (6, 8, 15, 255),
    "shadow": (5, 7, 14, 140),
    "asphalt_base": (18, 21, 30, 255),
    "asphalt_light": (26, 30, 44, 255),
    "asphalt_worn": (34, 40, 58, 255),
    "curb_dark": (43, 49, 69, 255),
    "curb_light": (74, 83, 107, 255),
    "guardrail": (120, 130, 155, 255),
    "line_white": (210, 225, 245, 255),
    "line_yellow": (255, 170, 0, 255),
    
    # Neon Accents
    "neon_cyan": (0, 229, 255, 255),
    "neon_cyan_glow": (0, 229, 255, 90),
    "neon_magenta": (255, 0, 127, 255),
    "neon_magenta_glow": (255, 0, 127, 90),
    "neon_violet": (138, 43, 226, 255),
    "neon_amber": (255, 170, 0, 255),
    "neon_red": (255, 34, 68, 255),
    "xenon_white": (240, 248, 255, 255),
    
    # Body Colors
    "kitsune_body_main": (24, 28, 48, 255),
    "kitsune_body_light": (44, 52, 86, 255),
    "kitsune_body_dark": (14, 16, 28, 255),
    "kitsune_outline": (8, 10, 18, 255),
    "kitsune_accent": (0, 229, 255, 255),
    "glass_dark": (15, 25, 35, 230),
    "glass_spec": (160, 210, 240, 200),
    "wheel_rim": (195, 155, 75, 255), # bronze rims
    "tire_rubber": (12, 14, 20, 255),
}

def create_meta_file(file_path, ppu=16):
    meta_path = file_path + ".meta"
    import hashlib
    guid = hashlib.md5(file_path.encode('utf-8')).hexdigest()
    meta_template = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 1
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMasterTextureLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 0
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaIsTransparency: 1
  spriteColliderType: 1
  textureType: 8
"""
    meta_content = meta_template.format(guid=guid, ppu=ppu)
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_content)

def make_kitsune_s13():
    # 48x80 canvas, car is 36 px wide x 68 px tall
    # Top-Down 3/4 Perspective: front hood slightly shorter, roof visible, rear fastback
    img = Image.new("RGBA", (48, 80), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    
    # Shadow offset (dx=0, dy=-2)
    d.rounded_rectangle([6, 12, 42, 74], radius=6, fill=PAL["shadow"])
    
    # Outer Body Outline (36x68, centered at x: 6..41, y: 6..73)
    # y=6 is front bumper, y=73 is rear spoiler
    d.rounded_rectangle([6, 6, 41, 73], radius=6, fill=PAL["kitsune_outline"])
    
    # Main Body Panels
    d.rounded_rectangle([7, 7, 40, 72], radius=5, fill=PAL["kitsune_body_main"])
    
    # Front Bumper & Air Dam (y: 6..12)
    d.rectangle([9, 7, 38, 10], fill=PAL["kitsune_body_dark"])
    d.rectangle([13, 8, 34, 10], fill=PAL["asphalt_base"]) # front intake
    # Cyan tow hook accent
    d.point((10, 8), fill=PAL["neon_cyan"])
    
    # Pop-up / Sleek Headlights (y: 11..14)
    d.rectangle([9, 11, 16, 14], fill=PAL["xenon_white"])
    d.rectangle([31, 11, 38, 14], fill=PAL["xenon_white"])
    d.point((12, 12), fill=PAL["neon_cyan"])
    d.point((35, 12), fill=PAL["neon_cyan"])
    
    # Front Hood with Dual Aero Creases (y: 15..32)
    d.line([(10, 15), (10, 31)], fill=PAL["kitsune_body_light"])
    d.line([(37, 15), (37, 31)], fill=PAL["kitsune_body_dark"])
    # Hood vent
    d.rectangle([19, 21, 28, 25], fill=PAL["kitsune_body_dark"])
    d.line([(20, 22), (27, 22)], fill=PAL["neon_cyan"])
    
    # Front Windshield (y: 33..43)
    d.polygon([(10, 42), (13, 33), (34, 33), (37, 42)], fill=PAL["glass_dark"])
    # Specular Glare across windshield
    d.line([(14, 34), (20, 41)], fill=PAL["glass_spec"], width=2)
    
    # Roof (y: 44..55)
    d.rounded_rectangle([13, 43, 34, 55], radius=2, fill=PAL["kitsune_body_light"])
    # Sunroof panel
    d.rectangle([17, 45, 30, 52], fill=PAL["kitsune_body_dark"])
    d.rectangle([18, 46, 29, 51], fill=PAL["glass_dark"])
    
    # Side Windows
    d.polygon([(9, 43), (12, 43), (12, 55), (9, 55)], fill=PAL["glass_dark"])
    d.polygon([(35, 43), (38, 43), (38, 55), (35, 55)], fill=PAL["glass_dark"])
    
    # Rear Window / Fastback Hatch (y: 56..64)
    d.polygon([(12, 56), (10, 64), (37, 64), (35, 56)], fill=PAL["glass_dark"])
    # Defroster lines
    d.line([(14, 58), (33, 58)], fill=(40, 55, 75, 200))
    d.line([(13, 61), (34, 61)], fill=(40, 55, 75, 200))
    
    # Rear Trunk Deck & Wide Rear Fenders (y: 65..69)
    d.rectangle([8, 65, 39, 69], fill=PAL["kitsune_body_main"])
    
    # Rear Full-Width Japanese Neon Taillights & Garnish (y: 70..72)
    d.rectangle([8, 70, 39, 72], fill=PAL["kitsune_body_dark"])
    d.rectangle([9, 71, 18, 72], fill=PAL["neon_red"])
    d.rectangle([29, 71, 38, 72], fill=PAL["neon_red"])
    # Center light bar (Kitsune signature)
    d.line([(19, 71), (28, 71)], fill=PAL["neon_magenta"])
    # Reverse lights / license plate glow
    d.rectangle([21, 72, 26, 72], fill=PAL["line_yellow"])
    
    # Ducktail / GT Wing (y: 73)
    d.line([(7, 73), (40, 73)], fill=PAL["kitsune_body_light"], width=1)
    
    # Wheels / Widebody Stance (4 wheels peeking)
    # Front-left:
    d.rectangle([6, 18, 8, 27], fill=PAL["tire_rubber"])
    d.line([(7, 21), (7, 24)], fill=PAL["wheel_rim"])
    # Front-right:
    d.rectangle([39, 18, 41, 27], fill=PAL["tire_rubber"])
    d.line([(40, 21), (40, 24)], fill=PAL["wheel_rim"])
    # Rear-left:
    d.rectangle([6, 56, 8, 66], fill=PAL["tire_rubber"])
    d.line([(7, 59), (7, 63)], fill=PAL["wheel_rim"])
    # Rear-right:
    d.rectangle([39, 56, 41, 66], fill=PAL["tire_rubber"])
    d.line([(40, 59), (40, 63)], fill=PAL["wheel_rim"])
    
    path = os.path.join(PROJECT_ROOT, "Vehicles/kitsune_s13.png")
    img.save(path)
    create_meta_file(path, 16)
    print("Saved KITSUNE S13")

def make_traffic_compact():
    # 40x64 canvas, car 30x54 px (turquoise / teal city hatchback)
    img = Image.new("RGBA", (40, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    
    teal_main = (18, 68, 80, 255)
    teal_light = (30, 110, 125, 255)
    teal_dark = (10, 42, 52, 255)
    
    # Shadow
    d.rounded_rectangle([5, 10, 35, 59], radius=5, fill=PAL["shadow"])
    # Outline
    d.rounded_rectangle([5, 5, 34, 58], radius=5, fill=PAL["kitsune_outline"])
    # Body
    d.rounded_rectangle([6, 6, 33, 57], radius=4, fill=teal_main)
    # Hood (short hatchback front)
    d.rectangle([8, 8, 31, 20], fill=teal_light)
    # Headlights
    d.rectangle([7, 7, 12, 10], fill=PAL["line_yellow"])
    d.rectangle([27, 7, 32, 10], fill=PAL["line_yellow"])
    # Windshield
    d.polygon([(9, 28), (11, 21), (28, 21), (30, 28)], fill=PAL["glass_dark"])
    d.line([(12, 22), (18, 27)], fill=PAL["glass_spec"])
    # Compact Roof
    d.rounded_rectangle([11, 29, 28, 45], radius=2, fill=teal_main)
    # Rear window
    d.rectangle([10, 46, 29, 52], fill=PAL["glass_dark"])
    # Taillights
    d.rectangle([7, 55, 12, 57], fill=PAL["neon_red"])
    d.rectangle([27, 55, 32, 57], fill=PAL["neon_red"])
    # Wheels
    d.rectangle([5, 14, 7, 21], fill=PAL["tire_rubber"])
    d.rectangle([32, 14, 34, 21], fill=PAL["tire_rubber"])
    d.rectangle([5, 42, 7, 49], fill=PAL["tire_rubber"])
    d.rectangle([32, 42, 34, 49], fill=PAL["tire_rubber"])
    
    path = os.path.join(PROJECT_ROOT, "Traffic/traffic_compact.png")
    img.save(path)
    create_meta_file(path, 16)
    print("Saved Traffic Compact")

def make_traffic_sedan():
    # 48x80 canvas, car 34x66 px (charcoal slate luxury sedan)
    img = Image.new("RGBA", (48, 80), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    
    sedan_main = (38, 44, 54, 255)
    sedan_light = (60, 70, 85, 255)
    sedan_dark = (22, 26, 34, 255)
    
    d.rounded_rectangle([7, 12, 40, 73], radius=6, fill=PAL["shadow"])
    d.rounded_rectangle([7, 7, 40, 72], radius=6, fill=PAL["kitsune_outline"])
    d.rounded_rectangle([8, 8, 39, 71], radius=5, fill=sedan_main)
    
    # Chrome Grille & Headlights
    d.rectangle([14, 8, 33, 11], fill=sedan_dark)
    d.line([(15, 9), (32, 9)], fill=PAL["curb_light"])
    d.rectangle([9, 9, 13, 12], fill=PAL["xenon_white"])
    d.rectangle([34, 9, 38, 12], fill=PAL["xenon_white"])
    
    # Long Hood
    d.rectangle([11, 13, 36, 30], fill=sedan_light)
    # Windshield
    d.polygon([(11, 41), (13, 31), (34, 31), (36, 41)], fill=PAL["glass_dark"])
    d.line([(14, 32), (22, 40)], fill=PAL["glass_spec"], width=2)
    # Roof
    d.rectangle([13, 42, 34, 54], fill=sedan_main)
    # Rear Window
    d.polygon([(13, 55), (11, 62), (36, 62), (34, 55)], fill=PAL["glass_dark"])
    # Trunk
    d.rectangle([10, 63, 37, 68], fill=sedan_light)
    # Taillights
    d.rectangle([9, 69, 16, 71], fill=PAL["neon_red"])
    d.rectangle([31, 69, 38, 71], fill=PAL["neon_red"])
    
    # Wheels
    d.rectangle([7, 18, 9, 27], fill=PAL["tire_rubber"])
    d.rectangle([38, 18, 40, 27], fill=PAL["tire_rubber"])
    d.rectangle([7, 53, 9, 62], fill=PAL["tire_rubber"])
    d.rectangle([38, 53, 40, 62], fill=PAL["tire_rubber"])
    
    path = os.path.join(PROJECT_ROOT, "Traffic/traffic_sedan.png")
    img.save(path)
    create_meta_file(path, 16)
    print("Saved Traffic Sedan")

def make_traffic_van():
    # 48x96 canvas, car 38x84 px (Japanese delivery / kei commercial van)
    img = Image.new("RGBA", (48, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    
    van_main = (180, 185, 195, 255) # commercial off-white
    van_light = (225, 230, 240, 255)
    van_dark = (110, 115, 125, 255)
    stripe_blue = (25, 80, 160, 255)
    
    d.rounded_rectangle([5, 12, 42, 89], radius=6, fill=PAL["shadow"])
    d.rounded_rectangle([5, 6, 42, 88], radius=6, fill=PAL["kitsune_outline"])
    d.rounded_rectangle([6, 7, 41, 87], radius=5, fill=van_main)
    
    # Flat Cab Front Bumper
    d.rectangle([9, 8, 38, 12], fill=van_dark)
    d.rectangle([7, 9, 12, 13], fill=PAL["xenon_white"])
    d.rectangle([35, 9, 40, 13], fill=PAL["xenon_white"])
    
    # Steep Windshield
    d.polygon([(8, 25), (10, 14), (37, 14), (39, 25)], fill=PAL["glass_dark"])
    d.line([(11, 15), (20, 24)], fill=PAL["glass_spec"], width=2)
    
    # Roof Ribs & Delivery Livery
    d.rectangle([8, 26, 39, 82], fill=van_light)
    # Side commercial blue stripe
    d.line([(7, 28), (7, 82)], fill=stripe_blue, width=2)
    d.line([(40, 28), (40, 82)], fill=stripe_blue, width=2)
    # Roof strengthening ribs
    for y in range(32, 80, 8):
        d.line([(12, y), (35, y)], fill=van_dark)
        
    # Vertical Taillights
    d.rectangle([6, 78, 8, 86], fill=PAL["neon_red"])
    d.rectangle([39, 78, 41, 86], fill=PAL["neon_red"])
    
    # Wheels
    d.rectangle([5, 19, 7, 30], fill=PAL["tire_rubber"])
    d.rectangle([40, 19, 42, 30], fill=PAL["tire_rubber"])
    d.rectangle([5, 66, 7, 77], fill=PAL["tire_rubber"])
    d.rectangle([40, 66, 42, 77], fill=PAL["tire_rubber"])
    
    path = os.path.join(PROJECT_ROOT, "Traffic/traffic_van.png")
    img.save(path)
    create_meta_file(path, 16)
    print("Saved Traffic Van")

def make_road_segments():
    # Standard 4-lane Highway Segment: 256 px wide x 64 px tall (repeats seamlessly vertically)
    # Left barrier: x=0..16, Lane 1: 16..72, Lane 2: 72..128, Center divider: 126..130, Lane 3: 128..184, Lane 4: 184..240, Right barrier: 240..256
    img = Image.new("RGBA", (256, 64), PAL["asphalt_base"])
    d = ImageDraw.Draw(img)
    
    # Texture noise & wet reflections
    for y in range(0, 64, 4):
        d.line([(16, y), (240, y)], fill=PAL["asphalt_light"])
    for y in range(2, 64, 4):
        d.line([(16, y), (240, y)], fill=PAL["asphalt_base"])
        
    # Asphalt tire grooves (darker worn tracks in lanes)
    for lane_center in [44, 100, 156, 212]:
        d.rectangle([lane_center - 12, 0, lane_center - 8, 63], fill=(13, 15, 22, 255))
        d.rectangle([lane_center + 8, 0, lane_center + 12, 63], fill=(13, 15, 22, 255))
        
    # Center Double Solid Yellow Lines
    d.line([(126, 0), (126, 63)], fill=PAL["line_yellow"], width=2)
    d.line([(130, 0), (130, 63)], fill=PAL["line_yellow"], width=2)
    
    # Dashed White Lines between lanes 1 & 2 (x=72) and lanes 3 & 4 (x=184)
    # 24 px on, 16 px off
    for dash_y in [4, 44]:
        d.rectangle([71, dash_y, 73, dash_y + 16], fill=PAL["line_white"])
        d.rectangle([183, dash_y, 185, dash_y + 16], fill=PAL["line_white"])
        
    # Outer Solid White Edge Lines
    d.line([(18, 0), (18, 63)], fill=PAL["line_white"], width=2)
    d.line([(238, 0), (238, 63)], fill=PAL["line_white"], width=2)
    
    # Left Sidewalk & Guardrail (x=0..16)
    d.rectangle([0, 0, 16, 63], fill=PAL["curb_dark"])
    d.rectangle([12, 0, 16, 63], fill=PAL["curb_light"]) # Curb edge bevel
    d.rectangle([2, 0, 6, 63], fill=PAL["guardrail"])
    # Guardrail yellow reflectors
    for y in [10, 42]:
        d.rectangle([3, y, 5, y + 6], fill=PAL["neon_amber"])
        
    # Right Sidewalk & Guardrail (x=240..256)
    d.rectangle([240, 0, 256, 63], fill=PAL["curb_dark"])
    d.rectangle([240, 0, 244, 63], fill=PAL["curb_light"])
    d.rectangle([250, 0, 254, 63], fill=PAL["guardrail"])
    for y in [10, 42]:
        d.rectangle([251, y, 253, y + 6], fill=PAL["neon_amber"])
        
    path = os.path.join(PROJECT_ROOT, "Road/road_straight_4lane.png")
    img.save(path)
    create_meta_file(path, 16)
    print("Saved Road Straight 4-Lane")
    
    # Finish Line Segment
    finish_img = img.copy()
    fd = ImageDraw.Draw(finish_img)
    # Checkered pattern across asphalt (x=18..238, y=24..40)
    tile_sz = 8
    for row, y_pos in enumerate(range(24, 40, tile_sz)):
        for col, x_pos in enumerate(range(18, 238, tile_sz)):
            is_white = (row + col) % 2 == 0
            color = PAL["neon_cyan"] if is_white else PAL["asphalt_base"]
            fd.rectangle([x_pos, y_pos, min(x_pos + tile_sz - 1, 237), y_pos + tile_sz - 1], fill=color)
    # Neon laser overhead line
    fd.line([(18, 23), (238, 23)], fill=PAL["neon_magenta"], width=2)
    fd.line([(18, 41), (238, 41)], fill=PAL["neon_magenta"], width=2)
    
    path_finish = os.path.join(PROJECT_ROOT, "Road/road_finish_line.png")
    finish_img.save(path_finish)
    create_meta_file(path_finish, 16)
    print("Saved Road Finish Line")

def make_buildings_and_props():
    # Neon Tower A (64x128 px, dark building with glowing Kanji sign ドリフト)
    tower = Image.new("RGBA", (64, 128), (14, 17, 26, 255))
    d = ImageDraw.Draw(tower)
    
    # Vertical window grid (dark navy / unlit with occasional lit amber/cyan offices)
    for wy in range(8, 120, 10):
        for wx in range(6, 58, 8):
            d.rectangle([wx, wy, wx + 4, wy + 6], fill=(22, 28, 42, 255))
            if (wx * wy) % 17 == 0:
                d.rectangle([wx, wy, wx + 4, wy + 6], fill=PAL["neon_amber"])
            elif (wx + wy) % 23 == 0:
                d.rectangle([wx, wy, wx + 4, wy + 6], fill=PAL["neon_cyan"])
                
    # Vertical Neon Billboard in Center (Magenta frame with cyan Kanji glyphs)
    d.rectangle([24, 16, 40, 104], fill=(10, 10, 18, 255))
    d.rectangle([24, 16, 40, 104], outline=PAL["neon_magenta"], width=2)
    # Stylized pixel Kanji "ドリフト" (Drift)
    # ド:
    d.line([(28, 22), (36, 22)], fill=PAL["neon_cyan"], width=2)
    d.line([(32, 22), (32, 34)], fill=PAL["neon_cyan"], width=2)
    d.line([(34, 26), (37, 30)], fill=PAL["neon_cyan"], width=1)
    # リ:
    d.line([(28, 44), (28, 54)], fill=PAL["neon_cyan"], width=2)
    d.line([(34, 40), (34, 56)], fill=PAL["neon_cyan"], width=2)
    d.line([(34, 56), (32, 59)], fill=PAL["neon_cyan"], width=2)
    # フ:
    d.line([(28, 68), (36, 68)], fill=PAL["neon_cyan"], width=2)
    d.line([(36, 68), (30, 78)], fill=PAL["neon_cyan"], width=2)
    # ト:
    d.line([(32, 86), (32, 98)], fill=PAL["neon_cyan"], width=2)
    d.line([(32, 91), (37, 95)], fill=PAL["neon_cyan"], width=2)
    
    path_tower = os.path.join(PROJECT_ROOT, "Buildings/building_neon_tower.png")
    tower.save(path_tower)
    create_meta_file(path_tower, 16)
    print("Saved Building Neon Tower")
    
    # Storefront B (64x96 px, Tokyo street store with magenta awning and warm shop window)
    shop = Image.new("RGBA", (64, 96), (20, 24, 36, 255))
    sd = ImageDraw.Draw(shop)
    # Upper floor apartments with air conditioning units
    for wy in [10, 30]:
        for wx in [10, 30, 50]:
            sd.rectangle([wx - 4, wy, wx + 4, wy + 12], fill=(30, 38, 55, 255))
            sd.rectangle([wx - 2, wy + 14, wx + 4, wy + 18], fill=(60, 68, 85, 255)) # AC unit
    # Magenta Shop Awning
    sd.rectangle([4, 54, 60, 62], fill=PAL["neon_magenta"])
    for x in range(4, 60, 8):
        sd.line([(x, 54), (x, 62)], fill=PAL["xenon_white"])
    # Warm Shop Front (24h convenience store / ramen shop)
    sd.rectangle([6, 64, 58, 92], fill=(255, 220, 150, 230))
    sd.rectangle([8, 66, 30, 90], fill=(255, 240, 200, 255)) # Glass entrance
    sd.rectangle([34, 66, 56, 80], fill=(18, 22, 34, 255)) # Counter silhouette
    
    path_shop = os.path.join(PROJECT_ROOT, "Buildings/building_shop.png")
    shop.save(path_shop)
    create_meta_file(path_shop, 16)
    print("Saved Building Shop")
    
    # Vending Machine Prop (16x28 px)
    vm = Image.new("RGBA", (16, 28), (0, 0, 0, 0))
    vd = ImageDraw.Draw(vm)
    vd.rectangle([1, 2, 14, 26], fill=(12, 14, 22, 255))
    vd.rectangle([2, 3, 13, 25], fill=(210, 35, 45, 255)) # Red Tokyo Coca-Cola style
    vd.rectangle([3, 5, 12, 14], fill=(230, 245, 255, 255)) # Illuminated can display
    # Tiny pixel cans
    for cx in [4, 7, 10]:
        vd.point((cx, 8), fill=PAL["neon_cyan"])
        vd.point((cx, 11), fill=PAL["neon_amber"])
    # Coin slot & dispenser
    vd.point((11, 17), fill=PAL["line_yellow"])
    vd.rectangle([4, 20, 11, 23], fill=(20, 20, 25, 255))
    
    path_vm = os.path.join(PROJECT_ROOT, "Props/prop_vending_machine.png")
    vm.save(path_vm)
    create_meta_file(path_vm, 16)
    print("Saved Prop Vending Machine")
    
    # Street Lamp Prop (16x32 px, warm sodium amber glow)
    lamp = Image.new("RGBA", (16, 32), (0, 0, 0, 0))
    ld = ImageDraw.Draw(lamp)
    ld.line([(8, 4), (8, 30)], fill=PAL["curb_light"], width=2)
    ld.line([(8, 4), (13, 4)], fill=PAL["curb_light"], width=2)
    ld.rectangle([11, 5, 14, 7], fill=PAL["neon_amber"])
    
    path_lamp = os.path.join(PROJECT_ROOT, "Props/prop_street_lamp.png")
    lamp.save(path_lamp)
    create_meta_file(path_lamp, 16)
    print("Saved Prop Street Lamp")

def make_vfx_assets():
    # Drift Smoke Sprite (32x32 puff)
    smoke = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    sd = ImageDraw.Draw(smoke)
    sd.ellipse([4, 4, 28, 28], fill=(90, 115, 145, 180))
    sd.ellipse([8, 8, 24, 24], fill=(175, 210, 240, 220))
    sd.ellipse([11, 11, 20, 20], fill=(240, 250, 255, 255))
    path_smoke = os.path.join(PROJECT_ROOT, "VFX/vfx_drift_smoke.png")
    smoke.save(path_smoke)
    create_meta_file(path_smoke, 16)
    
    # Skid mark segment (8x16)
    skid = Image.new("RGBA", (8, 16), (0, 0, 0, 0))
    skd = ImageDraw.Draw(skid)
    skd.rectangle([1, 0, 3, 15], fill=(10, 12, 18, 190))
    skd.rectangle([5, 0, 7, 15], fill=(10, 12, 18, 190))
    path_skid = os.path.join(PROJECT_ROOT, "VFX/vfx_skidmark.png")
    skid.save(path_skid)
    create_meta_file(path_skid, 16)
    
    # Speed Streak for Boost (4x32)
    streak = Image.new("RGBA", (4, 32), (0, 0, 0, 0))
    std = ImageDraw.Draw(streak)
    std.line([(2, 0), (2, 31)], fill=PAL["neon_cyan"], width=2)
    std.point((2, 0), fill=PAL["xenon_white"])
    path_streak = os.path.join(PROJECT_ROOT, "VFX/vfx_speed_streak.png")
    streak.save(path_streak)
    create_meta_file(path_streak, 16)
    
    # Headlight Beams (48x80 soft pixelated cone)
    cone = Image.new("RGBA", (48, 80), (0, 0, 0, 0))
    cd = ImageDraw.Draw(cone)
    # Left beam cone
    cd.polygon([(12, 75), (0, 0), (20, 0)], fill=(0, 229, 255, 45))
    cd.polygon([(12, 75), (4, 10), (18, 10)], fill=(240, 250, 255, 90))
    # Right beam cone
    cd.polygon([(36, 75), (28, 0), (48, 0)], fill=(0, 229, 255, 45))
    cd.polygon([(36, 75), (30, 10), (44, 10)], fill=(240, 250, 255, 90))
    path_cone = os.path.join(PROJECT_ROOT, "VFX/vfx_headlight_beam.png")
    cone.save(path_cone)
    create_meta_file(path_cone, 16)
    print("Saved VFX assets")

def make_ui_and_backgrounds():
    # Touch Button D-Pad Left (64x64)
    btn_l = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    bld = ImageDraw.Draw(btn_l)
    bld.rounded_rectangle([2, 2, 61, 61], radius=12, fill=(12, 16, 26, 175), outline=PAL["neon_cyan"], width=2)
    bld.polygon([(18, 32), (38, 18), (38, 46)], fill=PAL["neon_cyan"])
    path_btn_l = os.path.join(PROJECT_ROOT, "UI/ui_btn_left.png")
    btn_l.save(path_btn_l)
    create_meta_file(path_btn_l, 16)
    
    # Touch Button D-Pad Right (64x64)
    btn_r = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    brd = ImageDraw.Draw(btn_r)
    brd.rounded_rectangle([2, 2, 61, 61], radius=12, fill=(12, 16, 26, 175), outline=PAL["neon_cyan"], width=2)
    brd.polygon([(46, 32), (26, 18), (26, 46)], fill=PAL["neon_cyan"])
    path_btn_r = os.path.join(PROJECT_ROOT, "UI/ui_btn_right.png")
    btn_r.save(path_btn_r)
    create_meta_file(path_btn_r, 16)
    
    # Drift Handbrake Button (80x80)
    btn_d = Image.new("RGBA", (80, 80), (0, 0, 0, 0))
    bdd = ImageDraw.Draw(btn_d)
    bdd.ellipse([2, 2, 77, 77], fill=(22, 10, 28, 190), outline=PAL["neon_magenta"], width=3)
    # DRIFT text stylized
    bdd.ellipse([14, 14, 65, 65], outline=(255, 0, 127, 90), width=1)
    # Inner skid arc
    bdd.arc([20, 20, 60, 60], start=45, end=270, fill=PAL["neon_magenta"], width=4)
    path_btn_d = os.path.join(PROJECT_ROOT, "UI/ui_btn_drift.png")
    btn_d.save(path_btn_d)
    create_meta_file(path_btn_d, 16)
    
    # Boost Button (80x80)
    btn_b = Image.new("RGBA", (80, 80), (0, 0, 0, 0))
    bbd = ImageDraw.Draw(btn_b)
    bbd.ellipse([2, 2, 77, 77], fill=(8, 24, 34, 190), outline=PAL["neon_cyan"], width=3)
    # Lightning / Flame Bolt
    bbd.polygon([(42, 16), (26, 38), (38, 38), (32, 64), (54, 34), (42, 34)], fill=PAL["neon_cyan"])
    path_btn_b = os.path.join(PROJECT_ROOT, "UI/ui_btn_boost.png")
    btn_b.save(path_btn_b)
    create_meta_file(path_btn_b, 16)
    
    # Bar Backplate & Fill (128x16)
    bar_bg = Image.new("RGBA", (128, 16), (0, 0, 0, 0))
    bgd = ImageDraw.Draw(bar_bg)
    bgd.rounded_rectangle([0, 0, 127, 15], radius=4, fill=(10, 12, 20, 210), outline=PAL["curb_light"], width=1)
    path_bar_bg = os.path.join(PROJECT_ROOT, "UI/ui_bar_bg.png")
    bar_bg.save(path_bar_bg)
    create_meta_file(path_bar_bg, 16)
    
    bar_fill_cyan = Image.new("RGBA", (128, 16), (0, 0, 0, 0))
    bfd = ImageDraw.Draw(bar_fill_cyan)
    bfd.rounded_rectangle([2, 2, 125, 13], radius=2, fill=PAL["neon_cyan"])
    path_bar_cyan = os.path.join(PROJECT_ROOT, "UI/ui_bar_fill_cyan.png")
    bar_fill_cyan.save(path_bar_cyan)
    create_meta_file(path_bar_cyan, 16)
    
    bar_fill_magenta = Image.new("RGBA", (128, 16), (0, 0, 0, 0))
    mfd = ImageDraw.Draw(bar_fill_magenta)
    mfd.rounded_rectangle([2, 2, 125, 13], radius=2, fill=PAL["neon_magenta"])
    path_bar_magenta = os.path.join(PROJECT_ROOT, "UI/ui_bar_fill_magenta.png")
    bar_fill_magenta.save(path_bar_magenta)
    create_meta_file(path_bar_magenta, 16)

    # Master Skyline Background for Home & Garage (384x216, 16:9 pixel art)
    bg = Image.new("RGBA", (384, 216), PAL["void"])
    bgd = ImageDraw.Draw(bg)
    # Night sky gradient with purple smog
    for y in range(0, 130):
        t = y / 130.0
        r = int(6 + (28 - 6) * t)
        g = int(8 + (12 - 8) * t)
        b = int(15 + (45 - 15) * t)
        bgd.line([(0, y), (383, y)], fill=(r, g, b, 255))
        
    # Tokyo Tower silhouette in distance (magenta beacon)
    bgd.line([(290, 50), (290, 120)], fill=(40, 15, 35, 255), width=2)
    bgd.polygon([(282, 120), (290, 70), (298, 120)], fill=(40, 15, 35, 255))
    bgd.point((290, 48), fill=PAL["neon_red"])
    
    # Distant skyscraper silhouettes (dark navy)
    import random
    random.seed(42)
    cur_x = 0
    while cur_x < 384:
        bw = random.randint(18, 40)
        bh = random.randint(50, 110)
        by = 130 - bh
        bgd.rectangle([cur_x, by, cur_x + bw, 130], fill=(14, 18, 28, 255))
        # Add random lit windows
        for wy in range(by + 6, 126, 6):
            for wx in range(cur_x + 3, cur_x + bw - 2, 5):
                if random.random() < 0.18:
                    win_col = PAL["neon_cyan"] if random.random() < 0.4 else PAL["neon_amber"]
                    bgd.point((wx, wy), fill=win_col)
        cur_x += bw + 2
        
    # Elevated highway bridge across midground (y: 120..142)
    bgd.rectangle([0, 128, 383, 138], fill=(22, 28, 42, 255))
    bgd.line([(0, 128), (383, 128)], fill=PAL["curb_light"])
    bgd.line([(0, 138), (383, 138)], fill=PAL["guardrail"])
    # Pillars
    for px in range(30, 384, 80):
        bgd.rectangle([px, 138, px + 12, 175], fill=(16, 20, 30, 255))
        
    # Wet asphalt foreground parking lot / staging bay (y: 142..215)
    for y in range(142, 216):
        t = (y - 142) / 74.0
        r = int(14 + (28 - 14) * t)
        g = int(17 + (32 - 17) * t)
        b = int(26 + (46 - 26) * t)
        bgd.line([(0, y), (383, y)], fill=(r, g, b, 255))
        
    # Cyan and Magenta light reflections on wet pavement
    for rx in [80, 160, 240, 310]:
        col = PAL["neon_cyan_glow"] if rx in [80, 240] else PAL["neon_magenta_glow"]
        bgd.line([(rx - 15, 175), (rx + 15, 215)], fill=col, width=3)
        
    path_bg = os.path.join(PROJECT_ROOT, "Backgrounds/bg_neon_district_skyline.png")
    bg.save(path_bg)
    create_meta_file(path_bg, 16)
    print("Saved Neon District Skyline Background")
    
    # Garage Interior Background (384x216)
    garage = Image.new("RGBA", (384, 216), (10, 12, 18, 255))
    gd = ImageDraw.Draw(garage)
    # Industrial brick & corrugated metal back wall
    for y in range(0, 140, 8):
        gd.line([(0, y), (383, y)], fill=(18, 22, 32, 255))
    # Overhead Industrial Neon Tubes
    gd.rectangle([40, 14, 150, 18], fill=PAL["xenon_white"])
    gd.rectangle([35, 12, 155, 20], outline=PAL["neon_cyan"], width=1)
    gd.rectangle([230, 14, 340, 18], fill=PAL["xenon_white"])
    gd.rectangle([225, 12, 345, 20], outline=PAL["neon_cyan"], width=1)
    # Wall posters (Retro Japanese Drift Tuning)
    gd.rectangle([70, 35, 110, 75], fill=(30, 15, 25, 255), outline=PAL["neon_magenta"], width=1)
    gd.rectangle([140, 40, 175, 70], fill=(12, 28, 35, 255), outline=PAL["neon_cyan"], width=1)
    # Tool chests and tire stacks
    gd.rectangle([20, 95, 60, 140], fill=(140, 30, 35, 255)) # Red Snap-on style cabinet
    for ty in [90, 105, 120]:
        gd.rounded_rectangle([320, ty, 360, ty + 12], radius=4, fill=PAL["tire_rubber"], outline=PAL["kitsune_outline"])
    # Garage Floor (Polished concrete with yellow caution chevron border)
    gd.rectangle([0, 140, 383, 215], fill=(24, 28, 38, 255))
    # Hydraulic lift zone yellow caution stripes
    for cx in range(100, 280, 20):
        gd.polygon([(cx, 150), (cx + 10, 150), (cx - 5, 205), (cx - 15, 205)], fill=PAL["neon_amber"])
    gd.rectangle([115, 155, 265, 200], fill=(16, 20, 28, 255)) # Lift pad
    
    path_garage = os.path.join(PROJECT_ROOT, "Backgrounds/bg_garage_interior.png")
    garage.save(path_garage)
    create_meta_file(path_garage, 16)
    print("Saved Garage Interior Background")

def make_portrait_backgrounds():
    # 240x426 (9:16 Portrait Canvas)
    # 1. Skyline Portrait
    bg = Image.new("RGBA", (240, 426), PAL["void"])
    bgd = ImageDraw.Draw(bg)
    for y in range(0, 260):
        t = y / 260.0
        r = int(6 + (28 - 6) * t)
        g = int(8 + (12 - 8) * t)
        b = int(15 + (45 - 15) * t)
        bgd.line([(0, y), (239, y)], fill=(r, g, b, 255))

    # Metropolis beacon spire in distance (Fictional Japanese metropolis tower)
    bgd.line([(180, 80), (180, 220)], fill=(40, 15, 35, 255), width=2)
    bgd.polygon([(172, 220), (180, 110), (188, 220)], fill=(40, 15, 35, 255))
    bgd.point((180, 78), fill=PAL["neon_red"])

    # High-rise skyline silhouettes
    import random
    random.seed(99)
    cx = 0
    while cx < 240:
        bw = random.randint(22, 45)
        bh = random.randint(90, 210)
        by = 260 - bh
        bgd.rectangle([cx, by, cx + bw, 260], fill=(14, 18, 28, 255))
        for wy in range(by + 8, 256, 8):
            for wx in range(cx + 4, cx + bw - 3, 6):
                if random.random() < 0.22:
                    wcol = PAL["neon_cyan"] if random.random() < 0.45 else PAL["neon_amber"]
                    bgd.point((wx, wy), fill=wcol)
        cx += bw + 2

    # Elevated highway overpass
    bgd.rectangle([0, 250, 239, 265], fill=(22, 28, 42, 255))
    bgd.line([(0, 250), (239, 250)], fill=PAL["curb_light"])
    bgd.line([(0, 265), (239, 265)], fill=PAL["guardrail"])
    for px in [35, 115, 195]:
        bgd.rectangle([px, 265, px + 14, 310], fill=(16, 20, 30, 255))

    # Wet asphalt staging ground in foreground (y: 265..425)
    for y in range(265, 426):
        t = (y - 265) / 160.0
        r = int(14 + (28 - 14) * t)
        g = int(17 + (32 - 17) * t)
        b = int(26 + (46 - 26) * t)
        bgd.line([(0, y), (239, y)], fill=(r, g, b, 255))

    # Wet pavement neon reflections
    for rx in [50, 120, 190]:
        col = PAL["neon_cyan_glow"] if rx in [50, 190] else PAL["neon_magenta_glow"]
        bgd.line([(rx - 15, 320), (rx + 15, 420)], fill=col, width=4)

    path_pbg = os.path.join(PROJECT_ROOT, "Backgrounds/bg_neon_district_skyline_portrait.png")
    bg.save(path_pbg)
    create_meta_file(path_pbg, 16)
    print("Saved Portrait Skyline Background")

    # 2. Garage Portrait
    garage = Image.new("RGBA", (240, 426), (10, 12, 18, 255))
    gd = ImageDraw.Draw(garage)
    for y in range(0, 240, 8):
        gd.line([(0, y), (239, y)], fill=(18, 22, 32, 255))

    # Overhead neon fixtures
    gd.rectangle([30, 20, 105, 26], fill=PAL["xenon_white"])
    gd.rectangle([25, 18, 110, 28], outline=PAL["neon_cyan"], width=1)
    gd.rectangle([135, 20, 210, 26], fill=PAL["xenon_white"])
    gd.rectangle([130, 18, 215, 28], outline=PAL["neon_cyan"], width=1)

    # Wall posters
    gd.rectangle([20, 50, 75, 110], fill=(30, 15, 25, 255), outline=PAL["neon_magenta"], width=1)
    gd.rectangle([90, 60, 145, 110], fill=(12, 28, 35, 255), outline=PAL["neon_cyan"], width=1)
    gd.rectangle([160, 50, 220, 100], fill=(22, 20, 10, 255), outline=PAL["neon_amber"], width=1)

    # Tool cabinet and tires
    gd.rectangle([15, 150, 55, 240], fill=(140, 30, 35, 255))
    for ty in [140, 165, 190, 215]:
        gd.rounded_rectangle([190, ty, 230, ty + 18], radius=5, fill=PAL["tire_rubber"], outline=PAL["kitsune_outline"])

    # Floor with lift zone
    gd.rectangle([0, 240, 239, 425], fill=(24, 28, 38, 255))
    for cx in range(40, 200, 22):
        gd.polygon([(cx, 255), (cx + 12, 255), (cx - 8, 370), (cx - 20, 370)], fill=PAL["neon_amber"])
    gd.rectangle([50, 265, 190, 360], fill=(16, 20, 28, 255))

    path_pgarage = os.path.join(PROJECT_ROOT, "Backgrounds/bg_garage_interior_portrait.png")
    garage.save(path_pgarage)
    create_meta_file(path_pgarage, 16)
    print("Saved Portrait Garage Interior Background")

if __name__ == "__main__":
    make_kitsune_s13()
    make_traffic_compact()
    make_traffic_sedan()
    make_traffic_van()
    make_road_segments()
    make_buildings_and_props()
    make_vfx_assets()
    make_ui_and_backgrounds()
    make_portrait_backgrounds()
    print("=== All master pixel art assets generated successfully! ===")
