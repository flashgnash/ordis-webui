# OW3N
Character sheet app.

## Custom dice

Some game systems roll bespoke dice whose faces show text or an image instead of
plain numbers. `src/die.js` models these: every face has a numeric `value` used
in formulas, and may override its appearance with `text` and/or an `image`.

```js
import { face, customDie, renderFace } from "./src/die.js";

const hitDie = customDie([
  face(0, { image: "/faces/blank.png" }),
  face(1, { text: "hit" }),
  face(1, { image: "/faces/crit.png" }),
]);

face(1) + face(2);              // 3        — formulas treat a face as its number
renderFace(hitDie.roll());      // e.g. { type: "image", src: "/faces/crit.png", value: 1 }
```

- **In formulas** a face coerces to its `value` (`face(1) + face(2) === 3`).
- **On its own** call `.display()` / `renderFace()` to get a descriptor
  (`image`, `text`, or `number`) for the UI to render.

Run the tests with `npm test`.
