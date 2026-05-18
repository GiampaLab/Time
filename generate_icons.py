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

    img = Image.new("RGBA", (ss, ss), (0, 0, 0, 255))  # solid black square
    draw = ImageDraw.Draw(img)

    arm_w = r * 0.10

    def arm(angle_deg, length):
        """Draw arm from centre outward — no back stub, flat base at centre."""
        a = math.radians(angle_deg)
        tx = cx + length * math.sin(a)
        ty = cy - length * math.cos(a)
        w = int(arm_w)
        draw.line([(cx, cy), (tx, ty)], fill=(255, 255, 255, 255), width=w)
        # Round cap only at the tip
        cr = arm_w / 2
        draw.ellipse([tx - cr, ty - cr, tx + cr, ty + cr], fill=(255, 255, 255, 255))

    # Minute hand: 180° (straight down), longer
    arm(180, r * 0.72)
    # Hour hand: 225° (7:30 lower-left), shorter
    arm(225, r * 0.52)

    # Centre dot on top of both arms
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
