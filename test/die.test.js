import { test } from "node:test";
import assert from "node:assert/strict";
import { face, isFace, renderFace, customDie } from "../src/die.js";

test("a face behaves as its number in formulas", () => {
  assert.equal(face(1) + face(2), 3);
  assert.equal(face(3, { text: "crit" }) * 2, 6);
  assert.equal(Number(face(4, { image: "/x.png" })), 4);
});

test("a text face stringifies to its text but keeps its value", () => {
  const f = face(1, { text: "hit" });
  assert.equal(`${f}`, "hit");
  assert.equal(f.value, 1);
});

test("display() prefers image, then text, then the number", () => {
  assert.deepEqual(face(0, { image: "/blank.png" }).display(), {
    type: "image",
    src: "/blank.png",
    value: 0,
  });
  assert.deepEqual(face(2, { text: "hit" }).display(), {
    type: "text",
    text: "hit",
    value: 2,
  });
  assert.deepEqual(face(5).display(), { type: "number", value: 5 });
});

test("image wins over text when both are given", () => {
  const f = face(1, { text: "hit", image: "/hit.png" });
  assert.equal(f.display().type, "image");
});

test("renderFace handles both faces and plain numbers", () => {
  assert.deepEqual(renderFace(face(1, { text: "hit" })), {
    type: "text",
    text: "hit",
    value: 1,
  });
  assert.deepEqual(renderFace(7), { type: "number", value: 7 });
});

test("isFace only recognises real faces", () => {
  assert.equal(isFace(face(1)), true);
  assert.equal(isFace(1), false);
  assert.equal(isFace(null), false);
  assert.equal(isFace({ value: 1 }), false);
});

test("face rejects non-numeric values", () => {
  assert.throws(() => face("x"), TypeError);
  assert.throws(() => face(NaN), TypeError);
});

test("customDie accepts faces and bare numbers", () => {
  const d = customDie([face(0, { image: "/blank.png" }), 1, face(2, { text: "hit" })]);
  assert.equal(d.sides, 3);
  assert.ok(d.faces.every(isFace));
});

test("customDie rejects an empty face list", () => {
  assert.throws(() => customDie([]), /at least one face/);
  assert.throws(() => customDie(null), /at least one face/);
});

test("roll returns a landed face and is deterministic with a supplied rng", () => {
  const d = customDie([face(0, { text: "a" }), face(1, { text: "b" }), face(2, { text: "c" })]);
  assert.equal(d.roll(() => 0).value, 0);
  assert.equal(d.roll(() => 0.5).value, 1);
  assert.equal(d.roll(() => 0.999).value, 2);
});

test("a custom die's faces sum by value in a formula", () => {
  const d = customDie([face(1, { text: "hit" }), face(1, { image: "/hit.png" }), face(0, { text: "miss" })]);
  const total = d.faces.reduce((sum, f) => sum + f, 0);
  assert.equal(total, 2);
});
