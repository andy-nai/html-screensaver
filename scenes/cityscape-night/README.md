# Cityscape Night

A procedurally generated pixel-art city at night. It scales to any screen size and keeps animating gently:
twinkling stars, a drifting moon, windows and neon signs fading on and off, clouds, passing cars and
pedestrians, the odd shooting star, and, very occasionally, something else.

**Settings** live in [`config.js`](config.js): sky and building colours, how many buildings each
row has, how often cars, pedestrians and shooting stars appear, how lively the windows are, the frame-rate
cap, and so on. Edit and restart the screensaver (or reload the page). Anything you delete falls back to the
default built into the page.

A 24-hour clock sits in the top corner away from the moon, glowing like a neon sign. The `clock` section of the config sets its position, font, size and colours, or hides it with `show: false`.

Run `HtmlScreensaver-Setup.exe /debug /scene:cityscape-night` to try it full screen.

When opened in a browser, the number after `#` in the address is the city's seed; keep it to see the same
city again, or set `display.seed` in the config. These test keys work in a browser or in the `/debug` window
(in the real screensaver any key quits):

| Key | Does |
|---|---|
| R (or click) | new city |
| S | shooting star |
| C | car |
| P | pedestrian |
