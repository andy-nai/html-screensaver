"""Draws the app icon (a little monitor showing a pixel-art night city) and writes app.ico.

Pixel art, to match the scenes: a 32x32 drawing scaled up crisply for the big sizes, and a simpler
16x16 drawing for the small ones so it still reads in lists and the taskbar.
Only needs Python's standard library.  Run:  python make-icon.py
"""
import struct, zlib, pathlib

BEZEL, BEZEL_HI, STAND = (43, 45, 66), (78, 82, 112), (64, 67, 94)
RIM = (104, 109, 146)          # a lighter edge so the frame still stands out on dark backgrounds
CITY, MOON, STAR = (14, 11, 30), (243, 237, 210), (255, 255, 255)
WARM, COOL = (255, 211, 107), (169, 212, 255)


def blank(n):
    return [[(0, 0, 0, 0)] * n for _ in range(n)]


def lerp(a, b, t):
    return tuple(round(a[i] + (b[i] - a[i]) * t) for i in range(3))


def sky(stops, y):
    """Colour of the sky gradient at row y; stops = [(row, colour), ...]"""
    for (y0, c0), (y1, c1) in zip(stops, stops[1:]):
        if y <= y1:
            return lerp(c0, c1, max(0, min(1, (y - y0) / max(1, y1 - y0))))
    return stops[-1][1]


class Pixels:
    """Draws on an n x n image in design units of `unit` pixels (so a 32-unit design fills n = 32 * unit)."""

    def __init__(self, n, unit):
        self.img, self.u = blank(n), unit

    def rect(self, x0, y0, x1, y1, c, a=255):
        u = self.u
        for y in range(round(y0 * u), round(y1 * u)):
            for x in range(round(x0 * u), round(x1 * u)):
                if 0 <= x < len(self.img) and 0 <= y < len(self.img):
                    self.img[y][x] = (*c, a)

    def dot(self, x, y, c, a=255):
        self.rect(x, y, x + 1, y + 1, c, a)

    def clear(self, x, y):
        self.rect(x, y, x + 1, y + 1, (0, 0, 0), 0)


def design32(p):
    # monitor
    p.rect(1, 3, 31, 25, RIM)
    p.rect(2, 4, 30, 24, BEZEL)
    p.rect(2, 4, 30, 5, BEZEL_HI)
    for x, y in [(1, 3), (30, 3), (1, 24), (30, 24)]:
        p.clear(x, y)
    # screen: dusk sky
    stops = [(5, (20, 16, 58)), (10, (44, 27, 86)), (15, (116, 54, 104)), (18, (226, 110, 88)), (20, (244, 156, 102))]
    for y in range(5, 23):
        p.rect(3, y, 29, y + 1, sky(stops, y))
    # moon with a soft halo and a crater
    for x, y in [(21, 6), (24, 6), (20, 7), (25, 7), (20, 10), (25, 10), (21, 11), (24, 11)]:
        p.dot(x, y, (200, 190, 230), 110)
    p.rect(21, 7, 25, 11, MOON); p.rect(22, 6, 24, 7, MOON); p.rect(22, 11, 24, 12, MOON)
    p.rect(20, 8, 21, 10, MOON); p.rect(25, 8, 26, 10, MOON)
    p.dot(23, 9, (214, 206, 176))
    # stars
    for x, y in [(5, 7), (9, 10), (13, 6), (17, 9), (27, 13), (7, 13)]:
        p.dot(x, y, STAR)
    # skyline
    for x0, x1, top in [(3, 6, 16), (6, 9, 13), (9, 11, 17), (11, 15, 11), (15, 17, 15), (17, 20, 12),
                        (20, 23, 16), (23, 26, 14), (26, 29, 17)]:
        p.rect(x0, top, x1, 23, CITY)
    p.rect(13, 8, 14, 11, CITY)                              # antenna
    p.dot(13, 8, (255, 90, 90))                              # ...with its red light
    for x, y, c in [(4, 18, WARM), (7, 15, WARM), (7, 18, COOL), (12, 13, WARM), (13, 16, WARM), (12, 19, COOL),
                    (18, 14, WARM), (18, 17, WARM), (21, 18, COOL), (24, 16, WARM), (24, 19, WARM), (27, 19, WARM),
                    (4, 21, WARM), (13, 21, WARM), (19, 20, COOL)]:
        p.dot(x, y, c)
    # stand
    p.rect(14, 25, 18, 27, RIM)
    p.rect(15, 25, 17, 27, BEZEL)
    p.rect(10, 27, 22, 29, RIM)
    p.rect(11, 27, 21, 28, STAND)
    p.clear(10, 28); p.clear(21, 28)


def design16(p):
    p.rect(0, 1, 16, 13, RIM)
    p.rect(1, 1, 15, 2, BEZEL_HI)
    for x, y in [(0, 1), (15, 1), (0, 12), (15, 12)]:
        p.clear(x, y)
    stops = [(2, (20, 16, 58)), (6, (70, 36, 94)), (9, (226, 110, 88)), (10, (244, 156, 102))]
    for y in range(2, 12):
        p.rect(1, y, 15, y + 1, sky(stops, y))
    p.rect(10, 3, 12, 5, MOON)
    for x, y in [(3, 3), (6, 5), (14, 6)]:
        p.dot(x, y, STAR)
    for x0, x1, top in [(1, 4, 9), (4, 6, 7), (6, 8, 8), (8, 10, 6), (10, 12, 9), (12, 15, 8)]:
        p.rect(x0, top, x1, 12, CITY)
    for x, y in [(2, 10), (5, 8), (9, 7), (9, 10), (13, 9)]:
        p.dot(x, y, WARM)
    p.rect(7, 13, 9, 14, RIM)
    p.rect(4, 14, 12, 15, RIM)


def png(img):
    n = len(img)
    raw = b"".join(b"\x00" + bytes(v for px in row for v in px) for row in img)
    chunk = lambda t, d: struct.pack(">I", len(d)) + t + d + struct.pack(">I", zlib.crc32(t + d) & 0xFFFFFFFF)
    return (b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", n, n, 8, 6, 0, 0, 0))
            + chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b""))


def render(n):
    if n <= 24:
        p = Pixels(n, n / 16); design16(p)
    else:
        p = Pixels(n, n / 32); design32(p)
    return p.img


def main():
    here = pathlib.Path(__file__).parent
    sizes = [16, 20, 24, 32, 40, 48, 64, 96, 128, 256]
    images = [png(render(n)) for n in sizes]
    head = struct.pack("<HHH", 0, 1, len(sizes))
    offset = 6 + 16 * len(sizes)
    entries = b""
    for n, data in zip(sizes, images):
        entries += struct.pack("<BBBBHHII", n % 256, n % 256, 0, 0, 1, 32, len(data), offset)
        offset += len(data)
    (here / "app.ico").write_bytes(head + entries + b"".join(images))
    print("wrote app.ico:", ", ".join(map(str, sizes)))


if __name__ == "__main__":
    main()
