# Harbour at Dusk

A procedurally generated pixel-art harbour that slowly drifts between golden dusk and blue night and back
(20 minutes for the full cycle). The sun sinks into the sea, the stars come out, and the lights come on in
the town across the water, on the boats, at the end of the pier and in the lighthouse, whose beam sweeps
round. Boats bob at their moorings with wobbly reflections, the odd fishing boat crosses in the distance,
gulls fly over while it's still light, and, very occasionally, something else stirs out in the bay.

**Settings** live in [`config.js`](config.js): the length of the dusk cycle and where it starts, sky and
water colours for dusk and night, how many boats there are, how often boats and gulls pass, the lighthouse
beam's speed, and so on. Edit and restart the screensaver (or reload the page). Anything you delete falls
back to the default built into the page.

A 24-hour clock sits in the top corner away from the lighthouse. The `clock` section of the config sets its position, font, size and colours, or hides it with `show: false`.

Run `HtmlScreensaver-Setup.exe /debug /scene:harbour-dusk` to try it full screen.

When opened in a browser, the number after `#` in the address is the harbour's seed; keep it to see the same
harbour again, or set `display.seed` in the config. These test keys work in a browser or in the `/debug`
window (in the real screensaver any key quits):

| Key | Does |
|---|---|
| R (or click) | new harbour |
| B | boat crossing in the distance |
| G | gulls |
| T | jump half a dusk cycle (dusk ↔ night) |
