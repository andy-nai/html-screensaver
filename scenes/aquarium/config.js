// Aquarium — settings
//
// Edit values here and reload the page. Anything you delete falls back to the
// default built into index.html, so you only need to keep what you change.
// All colours are '#rrggbb'.
//
// Test keys: R new reef · F fish · S school · J jellyfish · B bubbles

window.AQUARIUM_CONFIG = {

  display: {
    targetRows: 200,          // art pixels across the screen's shorter side (lower = chunkier pixels)
    animate: true,            // false = a still image
    frameRate: 24,            // frames drawn per second; 0 = as fast as the screen refreshes
    seed: null,               // a number to always show the same reef; null = random (or the #seed in the URL)
    clickToRegenerate: true,  // click / press R for a new reef
  },

  water: {
    colors: {
      gradient: ['#3cb4d0', '#2a96c0', '#1f78ac', '#185e94', '#13487c', '#0e3666', '#0a2852'],  // surface → floor
      surface: '#9ae4f2',
      haze: '#1d5f8e',        // distant rocks and plants fade towards this
    },
  },

  lightRays: {
    count: [4, 7],            // (min, max)
    strength: 1,              // 0 = no rays
    colors: { ray: '#c4f6ff' },
  },

  seabed: {
    caustics: 1,              // strength of the rippling light on the sand (0 = off)
    colors: {
      sand: ['#d6c693', '#bcac7c', '#9a8c68'],  // top → bottom
      rocks: ['#56687a', '#425264', '#334050'],  // lit → shaded
      caustics: '#effcff',
    },
  },

  plants: {
    kelp: [3, 7],             // tall swaying kelp (min, max)
    seagrass: [5, 10],        // clumps of seagrass (min, max)
    colors: {
      kelp: ['#6c7c2c', '#5a6a24', '#7c8c36'], kelpLeaf: '#90a040',
      seagrass: ['#3a8a4a', '#4aa05a', '#2e7a40'],
    },
  },

  coral: {
    count: [6, 11],           // (min, max)
    colors: {
      branching: ['#ff7a8a', '#ff9a5a', '#c86ad8', '#ffb0c0'],
      brain: ['#d8a05a', '#b8c86a', '#e88a6a'],
      fan: ['#c84a8a', '#e86a5a', '#8a5ad8'],
      anemone: ['#ff9ad0', '#a8ffd8', '#ffd08a'],
    },
  },

  fish: {
    solo: [5, 9],             // fish swimming on their own (min, max)
    schools: [1, 2],          // schools of little fish (min, max)
    schoolSize: [10, 22],
    // how often each kind turns up (0 = never)
    species: { clown: 1, tang: 1, yellow: 1, sergeant: 1, grouper: 0.3 },
    colors: {
      clown:    { body: '#ff7a1a', stripe: '#ffffff', edge: '#1a1a1a' },
      tang:     { body: '#2b5fd9', mark: '#101830', tail: '#ffd23a' },
      yellow:   { body: '#ffd23a', belly: '#fff0a0' },
      sergeant: { body: '#d8e070', belly: '#e8eef0', bar: '#20242c' },
      grouper:  { body: '#8a6a4a', spot: '#5a4030', belly: '#b8946a' },
      school:   { body: '#a8d0ec', stripe: '#3ab0ff', tail: '#ff4a6a' },
    },
  },

  bubbles: {
    vents: [1, 3],            // streams of bubbles from the sand (min, max)
    everySeconds: 0.8,        // roughly one bubble per stream this often
    colors: { bubble: '#d8f6ff' },
  },

  jellyfish: {
    everySeconds: 60,         // one roll for a jellyfish per this many seconds
    chance: 0.4,
    colors: ['#e8a8ff', '#ffb0d8', '#a8d8ff'],
  },

  particles: {
    density: 1,               // drifting specks in the water (0 = none)
    colors: { speck: '#cfefff' },
  },

  clock: {
    show: true,
    hour12: false,            // true = 12-hour time with AM/PM
    seconds: false,
    position: 'auto',         // 'auto' = just under the surface, top right;
                              // or 'top-left', 'top', 'top-right', 'bottom-left', 'bottom', 'bottom-right'
    font: 'pixel',            // 'pixel' (5×7), 'small' (3×5) or 'segment' (LED digits)
    size: 1,                  // 2, 3… for a bigger clock
    blinkColon: false,        // blink the ':' every second
    drift: true,              // move a pixel or two each minute so it can't burn into the screen
    glow: 0.6,                // 0 = no glow
    colors: { text: '#e0fcff', glow: '#7ae8ff' },
  },
};
