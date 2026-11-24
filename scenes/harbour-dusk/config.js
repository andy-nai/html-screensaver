// Harbour at Dusk — settings
//
// Edit values here and reload the page. Anything you delete falls back to the
// default built into index.html, so you only need to keep what you change.
//
// The scene drifts slowly between golden dusk and blue night and back. Most
// things have a `dusk` and a `night` colour and blend between the two.
// All colours are '#rrggbb'.
//
// Test keys: R new harbour · B passing boat · G gulls · T jump half a dusk cycle

window.HARBOUR_CONFIG = {

  display: {
    targetRows: 200,          // art pixels across the screen's shorter side (lower = chunkier pixels)
    animate: true,            // false = a still image
    frameRate: 24,            // frames drawn per second; 0 = as fast as the screen refreshes
    seed: null,               // a number to always show the same harbour; null = random (or the #seed in the URL)
    clickToRegenerate: true,  // click / press R for a new harbour
  },

  dusk: {
    cycleMinutes: 20,         // one full dusk → night → dusk cycle
    startAt: null,            // 0 = golden dusk, 1 = night; null = somewhere random in the first half
  },

  sky: {
    colors: {
      // top of the sky → horizon; both lists need the same number of colours
      dusk:  ['#2b2b5f', '#45397a', '#6e4886', '#a35a80', '#d6706f', '#f0955f', '#ffbf6e'],
      night: ['#060a20', '#0a102b', '#0f1736', '#151f44', '#1d2a52', '#283660', '#35436c'],
    },
  },

  sun: {
    colors: { disc: '#ffe7a6', setting: '#ff8a52', glow: '#ffb070' },
  },

  stars: {
    count: 110,
    colors: ['#5a5f8f', '#9aa0cc', '#d8dcf6', '#ffffff'],  // dimmest → brightest
  },

  water: {
    tint: 0.6,                // how much the water's own colour covers the reflected sky (0–1)
    rippleDensity: 1,         // more or fewer glints on the water
    colors: {
      // horizon → bottom of the screen
      dusk:  ['#b86a70', '#8a5272', '#5c3f6a', '#3a2f5a', '#221f44'],
      night: ['#27345e', '#1d2850', '#151e42', '#0e1534', '#090e26'],
      glint: { dusk: '#ffd2a0', night: '#7f8fc4' },
    },
  },

  shore: {
    lightsPerHundredPixels: 9, // lights in the town across the water
    colors: {
      hills: { dusk: '#4e3463', night: '#111832' },
      rocks: { dusk: '#2e2240', night: '#0b0f22' },
      lights: ['#ffd27a', '#ffc35a', '#ffe7b0', '#ffb070', '#d8ecff'],
    },
  },

  lighthouse: {
    beamSeconds: 12,          // one full turn of the beam
    colors: { white: '#e9ddcd', red: '#b2403c', roof: '#5a2230', lamp: '#fff3b8', beam: '#fff0c0' },
  },

  boats: {
    moored: [3, 6],           // how many boats sit in the harbour (min, max)
    passingEverySeconds: 45,  // one roll for a boat crossing in the distance per this many seconds
    passingChance: 0.5,
    colors: {
      hulls: ['#e8e0d4', '#2f5a8a', '#a8423c', '#2e6a54', '#d8b25a', '#3a3a4a', '#f0f0f0'],
      trim: ['#2a2a3a', '#c8c0b0', '#8a2a2a', '#244a7a'],
      cabins: ['#e8e0d4', '#cfc6b6', '#f4efe6'],
      sail: '#ece4d4',
      mast: '#8a7a6a',
      lights: '#ffd890',
    },
  },

  gulls: {
    everySeconds: 20,         // one roll for a few gulls per this many seconds (fewer as it gets dark)
    chance: 0.5,
    colors: { body: '#2a2034' },
  },

  pier: {
    colors: { deck: '#6a4c3c', plank: '#563c30', post: '#33231e', lamp: '#ffd890' },
  },

  nightTint: { color: '#0a1030', amount: 0.55 },  // how much scenery darkens at night

  clock: {
    show: true,
    hour12: false,            // true = 12-hour time with AM/PM
    seconds: false,
    position: 'auto',         // 'auto' = top corner away from the lighthouse;
                              // or 'top-left', 'top', 'top-right', 'bottom-left', 'bottom', 'bottom-right'
    font: 'pixel',            // 'pixel' (5×7), 'small' (3×5) or 'segment' (LED digits)
    size: 1,                  // 2, 3… for a bigger clock
    blinkColon: false,        // blink the ':' every second
    drift: true,              // move a pixel or two each minute so it can't burn into the screen
    glow: 0.6,                // 0 = no glow
    colors: { text: '#fff0d8', glow: '#ffb070' },
  },
};
