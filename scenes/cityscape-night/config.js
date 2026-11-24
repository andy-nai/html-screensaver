// Cityscape Night — settings
//
// Edit values here and reload the page. Anything you delete falls back to the
// default built into index.html, so you only need to keep what you change.
// (This is a .js file rather than .json so it loads when the page is opened
// straight from disk.)
//
// Sections run roughly top of the picture to bottom. Each one has its settings
// first, then a `colors` block for that thing. All colours are '#rrggbb'.
//
// Test keys: R new city · S shooting star · C car · P pedestrian

window.CITYSCAPE_CONFIG = {

  display: {
    targetRows: 200,          // art pixels across the screen's shorter side (lower = chunkier pixels);
                              // a tall screen gets taller buildings, the street stays the same size
    animate: true,            // false = a still image
    frameRate: 24,            // frames drawn per second; lower saves power (30 still looks smooth
                              // for a scene this slow); 0 = as fast as the screen refreshes
    seed: null,               // a number to always show the same city; null = random (or the #seed in the URL)
    clickToRegenerate: true,  // click / press R for a new city
  },

  sky: {
    colors: {
      // top of the sky → horizon; as many as you like (at least 2), dithered together
      gradient: ['#05041a', '#0a0924', '#100e30', '#17143d', '#211a4b', '#2f2058', '#432862', '#5c3068', '#7a3a68'],
      haze: '#4b2e68',        // mist between the rows of buildings — keep it close to the horizon colour
      moonHalo: '#8f88c4',    // the soft dithered ring the moon lights up in the sky
    },
    // blue option:
    // colors: {
    //   gradient: ['#02051a', '#040a26', '#071133', '#0b1942', '#102252', '#162d62', '#1e3a71', '#28487e', '#35578a'],
    //   haze: '#22406e',
    //   moonHalo: '#8ea6d4',
    // },
  },

  moon: {
    pulseSeconds: 18,         // one full brighten-and-dim cycle of the moon's glow
    driftPixelsPerMinute: 2,  // how fast the moon creeps across the sky (0 = stay put);
                              // when it leaves one edge it comes back in from the other
    colors: { surface: '#f3edd2', shade: '#cdc39c', crater: '#b9ae88', glow: '#e9e2c8' },
  },

  stars: {
    colors: ['#4a4878', '#8481b8', '#c9c6ee', '#ffffff'],  // exactly 4: dimmest → brightest
  },

  shootingStars: {
    everyMinutes: 5,          // one roll for a shooting star per this many minutes
    chance: 0.75,             // chance that roll produces one
    colors: ['#ffffff', '#e4e2ff', '#a9a6dd', '#6e6aa8', '#3f3c74'],  // head → tail end
  },

  clouds: {
    clearNightChance: 0.4,    // chance a city has no clouds at all
    cloudiness: [0.4, 1],     // range for how cloudy a cloudy night is
    spacing: 24,              // roughly one cloud per this many art pixels of width at full cloudiness (lower = more)
    speed: [0.15, 0.45],      // drift speed range, art pixels per second
    spawnEverySeconds: 12,    // how often a new cloud may drift in from the edge
    colors: { body: '#2b2550', lit: '#3e3868', shade: '#221d44' },  // lit = moonlit tops, shade = undersides
    // blue option:
    // colors: { body: '#232e4f', lit: '#34436b', shade: '#1b2443' },
  },

  buildings: {
    // Share of building plots that actually have a building, per row:
    // 1 = all of them, 0.6 = about 60%, with empty gaps where the rest would be.
    // Building sizes don't change, and the ones that remain are the same as at
    // 1 for a given seed. Range 0–1. (A single number, e.g. count: 0.6, sets
    // all three rows at once.)
    count: {
      far:  0.6,              // back row
      mid:  0.6,              // middle row
      near: 0.6,              // front row
    },

    // Three rows, back to front. Keep the back row lightest (it's hazier and
    // further away) and the front row darkest, or the city loses its depth.
    //   body:        each building picks one of these
    //   edge:        thin moonlit outline on the side facing the moon
    //   darkWindows: colour of unlit windows (null = don't draw them)
    colors: {
      far:  { body: ['#221b47', '#251d4c', '#281f50'], edge: '#372d66', darkWindows: null },
      mid:  { body: ['#14112c', '#171331', '#1a1636'], edge: '#2a2453', darkWindows: '#1e1a3c' },
      near: { body: ['#0a0918', '#0c0a1c', '#0f0c21'], edge: '#1f1a3e', darkWindows: '#16132b' },
    },
    // blue option:
    // colors: {
    //   far:  { body: ['#1a2447', '#1c274c', '#1f2a50'], edge: '#2c3b66', darkWindows: null },
    //   mid:  { body: ['#0f152c', '#111831', '#131a36'], edge: '#212d53', darkWindows: '#171f3c' },
    //   near: { body: ['#070a18', '#080c1c', '#0a0e21'], edge: '#171f3e', darkWindows: '#10152b' },
    // },
  },

  windows: {
    switchEverySeconds: 30,   // each window (and neon sign) gets one chance to switch per this many seconds
    switchChance: 0.07,       // how lively they are; odds are balanced so each building keeps
                              // roughly the same brightness overall
    fadeSeconds: 1,           // how long a light takes to fade on or off
    // Each building picks one style; weight = how often it's chosen.
    // You can add your own styles here with any name.
    colors: {
      warm:   { weight: 6, shades: ['#ffd36b', '#ffc84f', '#ffe29a', '#f7b54a'] },
      cool:   { weight: 2, shades: ['#d6ecff', '#a9d4ff', '#c2e2ff'] },
      green:  { weight: 1, shades: ['#b4ffd9', '#8ff0c4'] },
      orange: { weight: 1, shades: ['#ffb38a', '#ff9f70'] },
    },
  },

  neonSigns: {
    colors: {
      tubes: ['#ff4fd8', '#3bf0ff', '#ff6a3b', '#7dff5a', '#ffe14f', '#b07bff'],  // each sign picks one
      body: '#0d0a1a',
    },
  },

  rooftopLights: {
    fadeSeconds: [8, 14],     // range for one slow fade in and out (each light picks its own)
    colors: { on: '#e04848', off: '#3a1424' },
  },

  street: {
    colors: { sidewalk: '#151229', curb: '#272144', road: '#0a0915', laneMarkings: '#2c2747' },
  },

  streetlights: {
    sidewalkSpacing: [22, 34],    // art pixels between the small lamps along the pavement
    foregroundSpacing: [38, 50],  // art pixels between the lamps nearest the viewer
    colors: {
      sidewalk:   { pole: '#3e3c4c', light: '#d4d2e2', glow: '#aeacc8' },
      foreground: { pole: '#4a4858', hoodTop: '#6c6a7c', light: '#b6b4c8', glow: '#a3a1bf' },
    },
  },

  cars: {
    everySeconds: 15,         // how often there's a roll for a car
    chance: 0.5,              // chance that roll sends a car down the street
    speed: [9, 16],           // art pixels per second
    taxiChance: 0.12,         // chance a car is a yellow taxi
    colors: {
      // body colours (darkened a little at night so they sit in the scene)
      bodies: ['#c0392b', '#2e86de', '#f5f6fa', '#27ae60', '#8e44ad', '#e67e22', '#7f8c8d', '#16a085', '#d35400', '#34495e'],
      headlights: '#fff1c8',
      taillights: '#ff3b3b',
      glass: '#1c2640',
    },
  },

  pedestrians: {
    everySeconds: 8,          // how often there's a roll for someone to walk by
    chance: 0.35,             // chance that roll sends a pedestrian along the pavement
    speed: [2.5, 4.5],        // walking speed, art pixels per second
    colors: {
      // each person picks one of each (darkened a little at night)
      coats: ['#8b3a3a', '#3a5a8b', '#5a7a4a', '#7a6a5a', '#b08a4a', '#5a4a7a', '#8a8a9a', '#2a3a4a', '#a05a7a'],
      skin:  ['#f1c9a5', '#e0ac85', '#c68863', '#9c6644', '#6b4430'],
      hair:  ['#1e1a1a', '#4a3020', '#7a5030', '#c8a060', '#9a9aa0', '#8a3a20'],
    },
  },
};
