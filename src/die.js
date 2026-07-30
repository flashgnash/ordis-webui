// Custom dice for OW3N.
//
// Some game systems roll bespoke dice whose faces show text or an image instead
// of plain numbers (Fudge dice, Genesys narrative dice, symbol/hero dice, ...).
// A face always carries a numeric `value` so it can still be used in a formula,
// but it may override how it looks when shown on its own with `text` or `image`.
//
//   const d = customDie([
//     face(0, { image: "/faces/blank.png" }),
//     face(1, { text: "hit" }),
//     2,
//   ]);
//
//   face(1) + face(2)        // => 3   (formulas treat a face as its number)
//   renderFace(face(1, { image: "/hit.png" }))
//                            // => { type: "image", src: "/hit.png", value: 1 }

const FACE = Symbol("ow3n.face");

// Build a single die face. `value` is the number used in formulas; `override`
// may carry `text` and/or an `image` to show when the face stands alone.
export function face(value, override = {}) {
  if (typeof value !== "number" || Number.isNaN(value)) {
    throw new TypeError("face value must be a number");
  }
  const { text = null, image = null } = override ?? {};

  const self = {
    [FACE]: true,
    value,
    text,
    image,

    // In a formula a face is just its number. Arithmetic (`+`, `*`, ...) and
    // loose equality use the "default"/"number" hints, so those yield the value
    // and `face(1) + face(2) === 3`. Only explicit string coercion (template
    // literals, `String(...)`) uses the "string" hint and shows the text face.
    valueOf() {
      return value;
    },
    [Symbol.toPrimitive](hint) {
      return hint === "string" ? self.toString() : value;
    },

    // Shown on its own, a face prefers its custom appearance and falls back to
    // the plain number. Returns a descriptor the UI can render however it likes.
    display() {
      if (image !== null) return { type: "image", src: image, value };
      if (text !== null) return { type: "text", text, value };
      return { type: "number", value };
    },

    toString() {
      if (text !== null) return text;
      return String(value);
    },
  };

  return self;
}

// True when `x` was produced by `face()`.
export function isFace(x) {
  return Boolean(x) && x[FACE] === true;
}

// Coerce a face (or a plain number) into a display descriptor. Use this when a
// single rolled result is shown by itself rather than summed into a formula.
export function renderFace(x) {
  if (isFace(x)) return x.display();
  return { type: "number", value: Number(x) };
}

// Build a custom die from a list of faces. Each entry may be a `face(...)` or a
// bare number, which is treated as a plain numeric face.
export function customDie(faces) {
  if (!Array.isArray(faces) || faces.length === 0) {
    throw new Error("a die needs at least one face");
  }
  const built = faces.map((f) => (isFace(f) ? f : face(f)));

  return {
    faces: built,
    sides: built.length,

    // Roll the die and return the landed face. Pass a custom `rng` (returning a
    // float in [0, 1)) to make rolls deterministic in tests.
    roll(rng = Math.random) {
      const i = Math.floor(rng() * built.length);
      return built[i];
    },
  };
}
