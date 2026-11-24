# Rainy Window

Looking out of a window on a rainy night at an out-of-focus city. Raindrops land on the glass, gather into
bigger drops and run down in fits and starts, wiping clear trails through the condensation. The rain comes
and goes in slow waves. Below, a wet road runs away from the window into the city, with headlights coming
closer and taillights fading into the distance. A candle flickers on the wooden windowsill, the glass has
the odd smudge and scratch, once in a while there's a flash of lightning, and, very occasionally, you have
company.

**Settings** live in [`config.js`](config.js): city colours and how blurred it is, how hard it rains, how
much condensation and grime there is on the glass, how often cars and lightning come, and whether to show the
window frame and the candle. Edit and restart the screensaver (or reload the page). Anything you delete falls back to the default
built into the page.

A 24-hour clock sits on the windowsill, as a little red LED bedside clock. The `clock` section of the config sets its position, font, size and colours, or hides it with `show: false`.

Run `HtmlScreensaver-Setup.exe /debug /scene:rainy-window` to try it full screen.

When opened in a browser, the number after `#` in the address is the view's seed; keep it to see the same
city again, or set `display.seed` in the config. These test keys work in a browser or in the `/debug`
window (in the real screensaver any key quits):

| Key | Does |
|---|---|
| R (or click) | new view |
| L | lightning |
| C | car |
| D | a drop runs down the glass |
