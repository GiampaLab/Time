/* ------------------------------------------------------------------ *
 * Skin benchmark harness.
 *
 * Runs only when the app is loaded with `?bench=1`. It exists to answer one
 * question with numbers rather than opinion: does revealing a screen-space
 * aurora through the clock arms cost more than painting the light inside each
 * arm, on the device that actually has to run this for hours.
 *
 * It is written for an Android DreamService, which sets isInteractive=false:
 * ANY touch exits the dream. So there is nothing to tap - the harness cycles
 * the skins itself, measures each one, and prints a table large enough to read
 * across a room. Results also go to console.log, which the dream service
 * forwards to logcat (tag TimeWebView), and to localStorage so a later ordinary
 * page load can show the last run.
 *
 * Two rules keep the measurement honest:
 *
 *   1. Identical workload. The C# orchestrator picks phases and patterns at
 *      random - correct for a screensaver, useless for a comparison, since two
 *      skins would be measured against different motion. In bench mode the
 *      orchestrator is not started at all (see AnalogClock.razor) and the
 *      harness drives all 48 arms itself with one deterministic infinite
 *      rotation. That is also the worst case: every arm moving, always.
 *
 *   2. The meter must not perturb what it measures. Frame timestamps are
 *      pushed into a preallocated array during a phase and nothing touches the
 *      DOM until the phase ends.
 * ------------------------------------------------------------------ */

(function () {
  "use strict";

  // Each variant names a skin class and says what it is there to tell us.
  var VARIANTS = [
    { key: "classic", cls: "", note: "floor: plain hands, no light at all" },
    { key: "aurora", cls: "theme-aurora", note: "control: today's per-arm aurora" },
    { key: "lens", cls: "theme-lens", note: "screen-space field + per-arm backdrop-filter" },
    { key: "field", cls: "theme-field", note: "screen-space field + multiply, no filtering" }
  ];

  /* Where the settings come from. In a browser they are query params:
       ?bench=1&measure=60000&rounds=6      - a long run for thermal behaviour
       ?bench=1&warmup=300&measure=800&rounds=1 - a quick smoke test

     The Android DreamService cannot use those: it calls loadUrl() on a bare
     "https://appassets.androidplatform.net/" with no query string, and it is
     not interactive, so there is no way to type one either. So a
     <meta name="bench"> in index.html enables the benchmark too, and its
     content is read exactly as a query string would be. Adding that one tag
     before ./build-android.sh turns the screensaver into the benchmark;
     removing it turns it back into a clock. No Kotlin change, no rebuild of
     anything but the assets. */
  function benchConfig() {
    var q = location.search || "";
    var meta = document.querySelector('meta[name="bench"]');
    if (meta) {
      q += (q ? "&" : "?") + (meta.getAttribute("content") || "bench=1");
    }
    return q;
  }

  function param(name, dflt) {
    var m = new RegExp("[?&]" + name + "=([^&]*)").exec(benchConfig());
    var v = m ? parseInt(decodeURIComponent(m[1]), 10) : NaN;
    return isNaN(v) ? dflt : v;
  }

  function isEnabled() {
    return /[?&]bench=1/.test(location.search) ||
      !!document.querySelector('meta[name="bench"]');
  }

  var WARMUP_MS = param("warmup", 3000); // discarded: layerisation, first paints
  var MEASURE_MS = param("measure", 12000);
  var ROUNDS = param("rounds", 3); // repeated so thermal drift shows as a trend
  var JANK_MS = param("jank", 20); // a frame this long is a visible hitch at 60Hz

  var overlay = null;
  var results = []; // [{round, key, stats}]

  function el(tag, cls) {
    var e = document.createElement(tag);
    if (cls) e.className = cls;
    return e;
  }

  function percentile(sorted, p) {
    if (!sorted.length) return 0;
    var i = Math.min(sorted.length - 1, Math.max(0, Math.round((p / 100) * (sorted.length - 1))));
    return sorted[i];
  }

  function summarise(deltas) {
    var sorted = deltas.slice().sort(function (a, b) { return a - b; });
    var total = 0;
    var jank = 0;
    for (var i = 0; i < deltas.length; i++) {
      total += deltas[i];
      if (deltas[i] > JANK_MS) jank++;
    }
    return {
      frames: deltas.length,
      fps: deltas.length > 0 ? 1000 / (total / deltas.length) : 0,
      p50: percentile(sorted, 50),
      p95: percentile(sorted, 95),
      p99: percentile(sorted, 99),
      worst: sorted.length ? sorted[sorted.length - 1] : 0,
      jankPct: deltas.length ? (100 * jank) / deltas.length : 0
    };
  }

  // Record frame-to-frame deltas for `ms`, touching nothing else meanwhile.
  function record(ms) {
    return new Promise(function (resolve) {
      var deltas = [];
      var last = 0;
      var started = 0;
      function tick(now) {
        if (!started) {
          started = now;
          last = now;
          requestAnimationFrame(tick);
          return;
        }
        deltas.push(now - last);
        last = now;
        if (now - started < ms) {
          requestAnimationFrame(tick);
        } else {
          resolve(deltas);
        }
      }
      requestAnimationFrame(tick);
    });
  }

  function wait(ms) {
    return new Promise(function (r) { setTimeout(r, ms); });
  }

  /* The deterministic workload. Every arm rotates forever; the periods are
     spread across a small range so the wall does not turn as one rigid block
     (which would let the compositor do less work than the real app ever does),
     but they are derived from the index, not random, so every variant and every
     round sees exactly the same motion. */
  function driveArms() {
    var arms = document.querySelectorAll(".clock .hour, .clock .minute");
    for (var i = 0; i < arms.length; i++) {
      arms[i].getAnimations().forEach(function (a) { a.cancel(); });
      arms[i].animate(
        [{ transform: "rotate(0deg)" }, { transform: "rotate(360deg)" }],
        {
          duration: 7000 + (i % 11) * 420,
          iterations: Infinity,
          easing: "linear",
          direction: i % 2 ? "reverse" : "normal"
        }
      );
    }
    return arms.length;
  }

  function setSkin(cls) {
    var wrapper = document.querySelector(".clocks-wrapper");
    wrapper.className = "clocks-wrapper" + (cls ? " " + cls : "");
    return wrapper;
  }

  function fmt(n, d) {
    return n.toFixed(d === undefined ? 1 : d);
  }

  function pad(s, w) {
    s = String(s);
    while (s.length < w) s += " ";
    return s;
  }

  function padL(s, w) {
    s = String(s);
    while (s.length < w) s = " " + s;
    return s;
  }

  /* The table. Rebuilt between phases only. Each skin's row shows the mean of
     its rounds; the per-round FPS follows it so a number that falls away as the
     device heats is visible rather than averaged into looking fine. */
  function render(current) {
    if (!overlay) {
      overlay = el("div", "bench-overlay");
      document.body.appendChild(overlay);
    }

    var lines = [];
    lines.push("SKIN BENCHMARK  " + VARIANTS.length + " skins x " + ROUNDS +
      " rounds x " + MEASURE_MS / 1000 + "s");
    lines.push("backdrop-filter: " + (supportsBackdrop() ? "supported" : "NOT SUPPORTED"));
    lines.push("");
    lines.push(pad("skin", 9) + padL("fps", 7) + padL("p50", 7) + padL("p95", 7) +
      padL("p99", 7) + padL("worst", 8) + padL("jank%", 7));
    lines.push("-".repeat(52));

    VARIANTS.forEach(function (v) {
      var mine = results.filter(function (r) { return r.key === v.key; });
      if (!mine.length) {
        lines.push(pad(v.key, 9) + padL(v.key === current ? "running" : "-", 7));
        return;
      }
      var avg = function (f) {
        return mine.reduce(function (a, r) { return a + f(r.stats); }, 0) / mine.length;
      };
      lines.push(
        pad(v.key, 9) +
        padL(fmt(avg(function (s) { return s.fps; })), 7) +
        padL(fmt(avg(function (s) { return s.p50; })), 7) +
        padL(fmt(avg(function (s) { return s.p95; })), 7) +
        padL(fmt(avg(function (s) { return s.p99; })), 7) +
        padL(fmt(avg(function (s) { return s.worst; })), 8) +
        padL(fmt(avg(function (s) { return s.jankPct; })), 7) +
        "   [" + mine.map(function (r) { return fmt(r.stats.fps, 0); }).join(" ") + "]"
      );
    });

    lines.push("");
    lines.push("times in ms. [..] = fps per round, in order: a falling");
    lines.push("trend is thermal throttling, not noise.");

    overlay.textContent = lines.join("\n");

    if (current) {
      var now = el("div", "bench-now");
      var v = VARIANTS.filter(function (x) { return x.key === current; })[0];
      now.textContent = "\n> measuring: " + current + " - " + (v ? v.note : "");
      overlay.appendChild(now);
    }
  }

  function supportsBackdrop() {
    return (window.CSS && CSS.supports &&
      (CSS.supports("backdrop-filter", "blur(1px)") ||
        CSS.supports("-webkit-backdrop-filter", "blur(1px)"))) || false;
  }

  function save() {
    try {
      localStorage.setItem("bench.results", JSON.stringify({
        at: new Date().toISOString(),
        ua: navigator.userAgent,
        backdrop: supportsBackdrop(),
        results: results
      }));
    } catch (e) {
      /* private mode, blocked storage - the on-screen table is the real output */
    }
  }

  async function run() {
    var armCount = driveArms();
    console.log("[bench] start; arms=" + armCount + "; backdrop-filter=" + supportsBackdrop());
    render(null);

    for (var round = 1; round <= ROUNDS; round++) {
      for (var i = 0; i < VARIANTS.length; i++) {
        var v = VARIANTS[i];
        setSkin(v.cls);
        render(v.key);
        await wait(WARMUP_MS);
        var deltas = await record(MEASURE_MS);
        var stats = summarise(deltas);
        results.push({ round: round, key: v.key, stats: stats });
        console.log("[bench] r" + round + " " + v.key +
          " fps=" + fmt(stats.fps) +
          " p50=" + fmt(stats.p50) +
          " p95=" + fmt(stats.p95) +
          " p99=" + fmt(stats.p99) +
          " worst=" + fmt(stats.worst) +
          " jank%=" + fmt(stats.jankPct));
        render(null);
        save();
      }
    }

    console.log("[bench] done");
    render(null);
    var done = el("div", "bench-now");
    done.textContent = "\n> finished - photograph this, or read it back with adb logcat -s TimeWebView";
    overlay.appendChild(done);

    // Leave the best-looking candidate on screen afterwards rather than
    // whatever happened to be last.
    setSkin("theme-lens");
  }

  window.benchHarness = {
    // Asked by AnalogClock.razor before it starts the orchestrator.
    isEnabled: isEnabled,
    start: function () {
      // OnAfterRenderAsync fires as soon as the DOM is up; give layout and the
      // first paints a moment to settle before the first warm-up starts.
      setTimeout(function () { run(); }, 500);
    },
    lastResults: function () {
      try {
        return JSON.parse(localStorage.getItem("bench.results"));
      } catch (e) {
        return null;
      }
    }
  };
})();
