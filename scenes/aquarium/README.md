# Aquarium

A procedurally generated pixel-art reef. Fish of several kinds swim across, now and then turning round,
schools of little fish weave about, kelp and seagrass sway, bubbles rise from the sand, and light from the
rippling surface shines down in rays and plays over the seabed. Occasionally a jellyfish pulses its way up,
and, very occasionally, something else drifts by.

**Settings** live in [`config.js`](config.js): water, sand and coral colours, how many plants, corals and
fish there are, which kinds of fish turn up and their colours, how strong the light rays and ripples are,
how often jellyfish visit, and so on. Edit and restart the screensaver (or reload the page).
Anything you delete falls back to the default built into the page.

A 24-hour clock sits just under the surface, top right. The `clock` section of the config sets its position, font, size and colours, or hides it with `show: false`.

Run `HtmlScreensaver-Setup.exe /debug /scene:aquarium` to try it full screen.

When opened in a browser, the number after `#` in the address is the reef's seed; keep it to see the same
reef again, or set `display.seed` in the config. These test keys work in a browser or in the `/debug`
window (in the real screensaver any key quits):

| Key | Does |
|---|---|
| R (or click) | new reef |
| F | another fish |
| S | another school |
| J | jellyfish |
| B | burst of bubbles |
