# Snowy Cabin

A procedurally generated pixel-art log cabin in a snowy pine forest at night. Firelight flickers in the
windows and spills onto the snow, smoke drifts from the chimney, and snow falls, getting heavier and lighter
in slow waves. Now and then a deer or a fox wanders across the clearing, leaving tracks that slowly fill
back in. Some nights the moon is out, some nights the northern lights ripple behind the mountains, and, very
occasionally, something else passes overhead.

**Settings** live in [`config.js`](config.js): sky, snow and tree colours, how hard it snows and which way
the wind blows, how often the moon and the northern lights appear, how often animals visit, how much the
firelight flickers, and so on. Edit and restart the screensaver (or reload the page). Anything you delete
falls back to the default built into the page.

A 24-hour clock sits in the top corner away from the moon. The `clock` section of the config sets its position, font, size and colours, or hides it with `show: false`.

Run `HtmlScreensaver-Setup.exe /debug /scene:snowy-cabin` to try it full screen.

When opened in a browser, the number after `#` in the address is the cabin's seed; keep it to see the same
cabin again, or set `display.seed` in the config. These test keys work in a browser or in the `/debug`
window (in the real screensaver any key quits):

| Key | Does |
|---|---|
| R (or click) | new cabin |
| D | deer |
| F | fox |
| A | northern lights on/off |
