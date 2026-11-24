// Rainy Window — settings
//
// Edit values here and reload the page. Anything you delete falls back to the
// default built into index.html, so you only need to keep what you change.
// All colours are '#rrggbb'.
//
// Test keys: R new view · L lightning · C car · D a drop runs down

window.RAIN_CONFIG = {

  display: {
    targetRows: 240,          // art pixels across the screen's shorter side (lower = chunkier pixels)
    animate: true,            // false = a still image
    frameRate: 24,            // frames drawn per second; 0 = as fast as the screen refreshes
    seed: null,               // a number to always show the same view; null = random (or the #seed in the URL)
    clickToRegenerate: true,  // click / press R for a new view
  },

  city: {
    blur: 3,                  // how out of focus the city behind the glass is (art pixels)
    bokeh: 40,                // soft out-of-focus lights in front of the city
    colors: {
      sky: ['#07081a', '#0e1026', '#191834', '#2a2042', '#3c2a4a'],  // top → street
      buildings: ['#14162a', '#0b0c18'],                              // far, near
      windows: ['#ffcf7a', '#ffe0a8', '#a8d0ff', '#ff9a6a'],
      lights: ['#ffd27a', '#ff6a5a', '#7ae0ff', '#ff5ad0', '#a0ff8a', '#ffb04a'],
    },
  },

  traffic: {
    // cars on the road running away from the window: headlights coming, taillights going
    everySeconds: 5,          // one roll for a car per this many seconds
    chance: 0.7,
    types: { sedan: 4, hatch: 3, suv: 3, van: 1.5, bus: 0.5 },  // how often each shape turns up (0 = never)
    nightShade: 0.45,         // how much darker the paint looks at night (0 = full daylight colour)
    colors: {
      headlights: '#fff2d0', taillights: '#ff3a3a',
      // paint colours; each car gets one, nudged slightly so no two match exactly
      bodies: ['#d8dce4', '#f0f0f4', '#a8aeb8', '#202028', '#5a5e66', '#b02028', '#1e4a8a', '#2e6a44', '#e0a820', '#6a3a1e', '#4a2e6a', '#2a8a9a'],
    },
  },

  rain: {
    intensity: 1,             // how hard it rains (0 = dry glass)
    heavyMinutes: 6,          // rain comes and goes over roughly this long
    lightest: 0.25,           // fraction of the full rain at its lightest
    maxDrops: 1,              // more or fewer droplets on the glass at once
    colors: { drop: '#aab6d6', highlight: '#ffffff', shadow: '#05060c' },
  },

  fog: {
    amount: 0.5,              // condensation on the glass (0 = clear); running drops wipe trails through it
    colors: { fog: '#8a96b8' },
  },

  glass: {
    smudges: 1,               // smudges, dust and fine scratches on the glass (0 = spotless)
    colors: { smudge: '#c8d0e8' },
  },

  lightning: {
    everyMinutes: 6,          // one roll for a flash per this many minutes
    chance: 0.3,
  },

  frame: {
    show: true,               // false = just the glass, edge to edge
    colors: { wood: '#2a1e1c', edge: '#3e2c28', sill: '#352624', sillTop: '#4a3430' },
  },

  candle: {
    show: true,               // a candle on the windowsill
    colors: { wax: '#efe6d6', flame: '#ffd27a', glow: '#ff9a40' },
  },

  clock: {
    show: true,
    hour12: false,            // true = 12-hour time with AM/PM
    seconds: false,
    position: 'auto',         // 'auto' = a bedside clock on the windowsill (bottom right without the frame);
                              // or 'top-left', 'top', 'top-right', 'bottom-left', 'bottom', 'bottom-right'
    font: 'segment',          // 'pixel' (5×7), 'small' (3×5) or 'segment' (LED digits)
    size: 1,                  // 2, 3… for a bigger clock
    blinkColon: true,         // blink the ':' every second
    drift: true,              // move a pixel or two each minute so it can't burn into the screen
    glow: 1,                  // 0 = no glow
    colors: { text: '#ff4030', glow: '#ff2a1a', box: '#161214' },
  },
};
