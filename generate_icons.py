#!/usr/bin/env python3
"""Generate clock icon at 7:30 for Android mipmaps and favicon."""
import math
import os
from PIL import Image, ImageDraw

def create_clock_icon(size):
    scale = 4
    ss = size * scale
    cx = cy = ss / 2
    r = ss / 2

    img = Image.new("RGBA", (ss, ss), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Black circle background
    draw.ellipse([0, 0, ss - 1, ss - 1], fill=(0, 0, 0, 255))

    arm_w = r * 0.10

    def arm(angle_deg, front, back):
        a = math.radians(angle_deg)
        tx = cx + front * r * math.sin(a)
        ty = cy - front * r * math.cos(a)
        bx = cx - back * r * math.sin(a)
        by = cy + back * r * math.cos(a)
        w = int(arm_w)
        draw.line([(bx, by), (tx, ty)], fill=(255, 255, 255, 255), width=w)
        cr = arm_w / 2
        for px, py in [(tx, ty), (bx, by)]:
            draw.ellipse([px - cr, py - cr, px + cr, py + cr], fill=(255, 255, 255, 255))

    # Minute hand: straight down (180°), longer
    arm(180, 0.74, 0.14)
    # Hour hand: 7:30 position (225°), shorter
    arm(225, 0.54, 0.12)

    # Centre dot
    dr = arm_w * 1.1
    draw.ellipse([cx - dr, cy - dr, cx + dr, cy + dr], fill=(255, 255, 255, 255))

    return img.resize((size, size), Image.LANCZOS)


android_sizes = {
    "mipmap-mdpi":    48,
    "mipmap-hdpi":    72,
    "mipmap-xhdpi":   96,
    "mipmap-xxhdpi":  144,
    "mipmap-xxxhdpi": 192,
}

res_root = "android/app/src/main/res"
for folder, size in android_sizes.items():
    path = os.path.join(res_root, folder)
    os.makedirs(path, exist_ok=True)
    icon = create_clock_icon(size)
    out = os.path.join(path, "ic_launcher.png")
    icon.save(out, "PNG")
    print(f"  {out}  ({size}x{size})")

# Favicon (web)
favicon = create_clock_icon(32)
favicon.save("wwwroot/favicon.png", "PNG")
print("  wwwroot/favicon.png  (32x32)")

print("Done.")
