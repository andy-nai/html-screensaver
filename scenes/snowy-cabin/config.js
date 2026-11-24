// Snowy Cabin — settings
//
// Edit values here and reload the page. Anything you delete falls back to the
// default built into index.html, so you only need to keep what you change.
// All colours are '#rrggbb'.
//
// Test keys: R new cabin · D deer · F fox · A northern lights on/off

window.CABIN_CONFIG = {

  display: {
    targetRows: 200,          // art pixels across the screen's shorter side (lower = chunkier pixels)
    animate: true,            // false = a still image
    frameRate: 24,            // frames drawn per second; 0 = as fast as the screen refreshes
    seed: null,               // a number to always show the same cabin; null = random (or the #seed in the URL)
    clickToRegenerate: true,  // click / press R for a new cabin
  },

  sky: {
    colors: {
      gradient: ['#050a1e', '#081028', '#0c1733', '#111f40', '#18294d', '#22355a', '#2e4366'],  // top → horizon
    },
  },

  moon: {
    chance: 0.75,             // chance a night has the moon out
    colors: { surface: '#eef0f6', shade: '#c4cad8', glow: '#b8c8ec' },
  },

  stars: {
    count: 130,
    colors: ['#4a5584', '#8a96c4', '#cdd6f2', '#ffffff'],  // dimmest → brightest
  },

  aurora: {
    chance: 0.3,              // chance a night has the northern lights
    speed: 1,                 // how quickly the curtains ripple
    colors: ['#3cf0a0', '#28d8c0', '#7a6cff'],
  },

  snowfall: {
    density: 1,               // more or fewer flakes
    gustMinutes: 5,           // snowfall swells and eases over roughly this long
    lightest: 0.2,            // fraction of the flakes still falling at the lightest point
    wind: 0.5,                // sideways drift (negative = the other way)
    colors: { flakes: '#eef4ff' },
  },

  mountains: {
    colors: { rock: '#2a3658', snow: '#a8b8d8', shade: '#6a7ca4' },
  },

  trees: {
    density: 1,
    colors: {
      far:  { needles: '#1a2846', snow: '#5a6c94' },
      mid:  { needles: '#0f1c30', snow: '#8a9cc0' },
      near: { needles: '#081220', snow: '#b4c4e0' },
      trunk: '#2a1e1e',
    },
  },

  ground: {
    colors: {
      // far → near snow; moonlit snow is blue
      snow: ['#3a4c74', '#4a5e88', '#5a709a', '#6c82ac', '#7e94bc'],
      sparkle: '#ffffff',
    },
  },

  cabin: {
    flicker: 0.25,            // how much the firelight in the windows flickers (0 = steady)
    colors: {
      logs: ['#5a3a28', '#4a3020'], gaps: '#24160f', door: '#3a2418',
      roofSnow: '#dce6f6', roofShade: '#9aaccc', chimney: '#5a5660',
      window: '#ffc566', windowGlow: '#ff9c40', lantern: '#ffd890',
    },
  },

  smoke: {
    amount: 1,                // 0 = no chimney smoke
    colors: { puff: '#8a96b4' },
  },

  animals: {
    everySeconds: 40,         // one roll for a visitor per this many seconds
    chance: 0.45,
    footprintMinutes: 3,      // how long tracks take to fill back in with snow
    colors: {
      deer: '#7a5638', deerLight: '#c8a880', antlers: '#d8c8a8',
      fox: '#d0662a', foxLight: '#f4ece0', dark: '#1a1418',
      tracks: '#4a5c86',
    },
  },

  clock: {
    show: true,
    hour12: false,            // true = 12-hour time with AM/PM
    seconds: false,
    position: 'auto',         // 'auto' = top corner away from the moon;
                              // or 'top-left', 'top', 'top-right', 'bottom-left', 'bottom', 'bottom-right'
    font: 'pixel',            // 'pixel' (5×7), 'small' (3×5) or 'segment' (LED digits)
    size: 1,                  // 2, 3… for a bigger clock
    blinkColon: false,        // blink the ':' every second
    drift: true,              // move a pixel or two each minute so it can't burn into the screen
    glow: 0.7,                // 0 = no glow
    colors: { text: '#eef4ff', glow: '#9ab8ff' },
  },
};
