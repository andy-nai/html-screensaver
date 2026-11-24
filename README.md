# html-screensaver

Slow, relaxing animated scenes that run in a browser, each a single HTML page drawn on a canvas. No build step and no dependencies: open a page and leave it running.

## Scenes

### Cityscape Night — `cityscape.html`

A procedurally generated pixel-art city at night. It scales to any window size and keeps animating gently: twinkling stars, a drifting moon, windows and neon signs fading on and off, clouds, passing cars and pedestrians, the odd shooting star, and — very occasionally — something else.

**Settings** live in [`config.js`](config.js): sky and building colours, how many buildings each row has, how often cars, pedestrians and shooting stars appear, how lively the windows are, and so on. Edit and reload. Anything you delete falls back to the default built into the page.

The number after `#` in the address is the city's seed. Keep it to see the same city again, or set `display.seed` in the config.

| Key | Does |
|---|---|
| R (or click) | new city |
| S | shooting star |
| C | car |
| P | pedestrian |
