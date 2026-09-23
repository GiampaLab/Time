/* ------------------------------------------------------------------ *
 * Skin benchmark harness.
 *
 * This file holds two diagnostics. The benchmark below, and a field-reveal
 * control (see the end of the file) that makes the lens skin's normally
 * invisible aurora field visible so its movement can be watched.
 *
 * The benchmark runs only when the app is loaded with `?bench=1`. It answers one
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
    return /[?&](bench|probe)=1/.test(benchConfig()) ||
      !!document.querySelector('meta[name="bench"]');
  }

  function isProbe() {
    return /[?&]probe=1/.test(benchConfig());
  }

  /* ---------------------------------------------------------------- *
   * Perf probe (?probe=1).
   *
   * The benchmark compares SKINS. This compares the levers WITHIN the lens
   * skin, so the device itself can say which are worth paying for. Every
   * entry after the baseline costs something visually - that is the point;
   * the question is what each one buys.
   *
   * Worth running because the same comparison in headless Chromium, with no
   * GPU, found nothing free. The off-screen overhang, an extra gradient, the
   * blend mode, a tiled bitmap and 96 pseudo-elements all measured as noise:
   * Chromium already skips off-screen tiles, Skia already fuses adjacent
   * colour matrices, and a solid colour is only fast because it needs no
   * texture at all. What did move it there was the tilt, the blur and the
   * second curtain - every one of which costs quality.
   *
   * The tilt used to be on this list and is not any more: the diagonal now
   * comes from the gradient's own angle rather than from rotating the layer,
   * which removed the resampling and its 1.3x without costing anything.
   *
   * A GPU reorders what is left: a per-arm backdrop blur is the expensive one
   * on real hardware. So do not port the desktop conclusions - run this and
   * read the device's own answer.
   * ---------------------------------------------------------------- */
  var PROBES = [
    { key: "baseline", css: "", note: "lens exactly as it ships" },
    { key: "blur 4px", note: "backdrop blur down from 10px",
      css: ".theme-lens .clock .hour div,.theme-lens .clock .minute div{" +
        "-webkit-backdrop-filter:blur(4px) saturate(2.1) brightness(14);" +
        "backdrop-filter:blur(4px) saturate(2.1) brightness(14)}" },
    { key: "no blur", note: "amplify only; risks banding",
      css: ".theme-lens .clock .hour div,.theme-lens .clock .minute div{" +
        "-webkit-backdrop-filter:saturate(2.1) brightness(14);" +
        "backdrop-filter:saturate(2.1) brightness(14)}" },
    { key: "no wash", note: "rays stop changing colour as they travel",
      css: ".aurora-field::after{display:none}" },
    { key: "no clip", note: "arms' outer glow no longer cropped to the face",
      css: ".theme-lens .clock{overflow:visible;border-radius:0}" },
    { key: "no filter", note: "floor: field present, but no arm lenses it",
      css: ".theme-lens .clock .hour div,.theme-lens .clock .minute div{" +
        "-webkit-backdrop-filter:none;backdrop-filter:none}" }
  ];

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

  /* ---------------------------------------------------------------- *
   * Field reveal.
   *
   * .theme-lens paints its aurora field at about 4% over black - low enough to
   * read as black, high enough for backdrop-filter to have something to
   * amplify. That is the point of the skin and it is also why the field's
   * movement is impossible to study: you only ever see it through the arms,
   * a few degrees of arc at a time.
   *
   * `f` cycles the field up through 20%, 50% and 100% and back to normal;
   * `?field=0.5` sets it at load. Nothing here changes the shipped look - the
   * default is untouched and the override is an inline style on the wrapper.
   *
   * Raising the field alone would blow the arms out to white, since brightness
   * is tuned for a 4% backdrop. So the arms' amplification is scaled down by
   * the same factor: the background becomes visible while the arms keep
   * looking roughly as they should, and you can see how the two relate.
   * ---------------------------------------------------------------- */
  var REVEAL_STEPS = [null, 0.2, 0.5, 1]; // null = the skin's own default
  var revealIndex = 0;
  var BASE_OPACITY = 0.04; // keep in step with --field-opacity in app.css
  var BASE_BRIGHTNESS = 14; // and with --lens-brightness

  function applyReveal(value) {
    var wrapper = document.querySelector(".clocks-wrapper");
    if (!wrapper) return;
    if (value === null || value === undefined) {
      wrapper.style.removeProperty("--field-opacity");
      wrapper.style.removeProperty("--lens-brightness");
      flash("field: default (invisible)");
      return;
    }
    wrapper.style.setProperty("--field-opacity", String(value));
    // Hold the arms at roughly their normal exposure as the field comes up.
    wrapper.style.setProperty(
      "--lens-brightness",
      String(Math.max(1, BASE_BRIGHTNESS * (BASE_OPACITY / value)))
    );
    flash("field: " + Math.round(value * 100) + "%");
  }

  var flashEl = null;
  var flashTimer = null;
  function flash(text) {
    if (!flashEl) {
      flashEl = el("div", "bench-overlay");
      flashEl.style.top = "auto";
      flashEl.style.bottom = "0";
      document.body.appendChild(flashEl);
    }
    flashEl.textContent = text + "   (f cycles)";
    flashEl.style.display = "block";
    clearTimeout(flashTimer);
    flashTimer = setTimeout(function () { flashEl.style.display = "none"; }, 1800);
  }

  function installReveal() {
    var m = /[?&]field=([^&]*)/.exec(location.search);
    if (m) {
      var v = parseFloat(decodeURIComponent(m[1]));
      if (!isNaN(v)) setTimeout(function () { applyReveal(v); }, 400);
    }
    window.addEventListener("keydown", function (e) {
      if (e.key !== "f" && e.key !== "F") return;
      revealIndex = (revealIndex + 1) % REVEAL_STEPS.length;
      applyReveal(REVEAL_STEPS[revealIndex]);
    });
  }

  installReveal();

  var probeStyle = null;
  function applyProbe(css) {
    if (!probeStyle) {
      probeStyle = document.createElement("style");
      document.head.appendChild(probeStyle);
    }
    probeStyle.textContent = css;
  }

  function median(a) {
    var s = a.slice().sort(function (x, y) { return x - y; });
    return s[Math.floor(s.length / 2)];
  }

  function renderProbe(results, current) {
    if (!overlay) {
      overlay = el("div", "bench-overlay");
      document.body.appendChild(overlay);
    }
    var base = results.baseline && results.baseline.length ? median(results.baseline) : 0;
    var lines = ["PERF PROBE - lens skin, " + ROUNDS + " x " + MEASURE_MS / 1000 + "s each", ""];
    lines.push(pad("variant", 12) + padL("fps", 7) + padL("vs base", 9) + "   what it costs");
    lines.push("-".repeat(64));
    PROBES.forEach(function (v) {
      var got = results[v.key];
      if (!got || !got.length) {
        lines.push(pad(v.key, 12) + padL(v.key === current ? "running" : "-", 7));
        return;
      }
      var m = median(got);
      lines.push(pad(v.key, 12) + padL(fmt(m), 7) +
        padL(base ? fmt(m / base, 2) + "x" : "-", 9) + "   " + v.note);
    });
    lines.push("");
    lines.push("at or below 1.00x is not worth its quality cost.");
    overlay.textContent = lines.join("\n");
    if (current) {
      var now = el("div", "bench-now");
      now.textContent = "\n> measuring: " + current;
      overlay.appendChild(now);
    }
  }

  async function runProbe() {
    setSkin("theme-lens");
    var armCount = driveArms();
    console.log("[probe] start; arms=" + armCount);
    var results = {};
    PROBES.forEach(function (v) { results[v.key] = []; });
    renderProbe(results, null);
    for (var round = 1; round <= ROUNDS; round++) {
      for (var i = 0; i < PROBES.length; i++) {
        var v = PROBES[i];
        applyProbe(v.css);
        renderProbe(results, v.key);
        await wait(WARMUP_MS);
        var stats = summarise(await record(MEASURE_MS));
        results[v.key].push(stats.fps);
        console.log("[probe] r" + round + " " + v.key + " fps=" + fmt(stats.fps) +
          " p95=" + fmt(stats.p95) + " jank%=" + fmt(stats.jankPct));
        renderProbe(results, null);
      }
    }
    applyProbe("");
    console.log("[probe] done");
    renderProbe(results, null);
    var done = el("div", "bench-now");
    done.textContent = "\n> finished - photograph this, or adb logcat -s TimeWebView";
    overlay.appendChild(done);
  }

  window.benchHarness = {
    reveal: applyReveal,
    // Asked by AnalogClock.razor before it starts the orchestrator.
    isEnabled: isEnabled,
    start: function () {
      // OnAfterRenderAsync fires as soon as the DOM is up; give layout and the
      // first paints a moment to settle before the first warm-up starts.
      setTimeout(function () { isProbe() ? runProbe() : run(); }, 500);
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
