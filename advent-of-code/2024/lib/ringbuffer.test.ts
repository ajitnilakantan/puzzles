import { describe, expect, test } from "bun:test";
import { RingBuffer } from "./ringbuffer";

describe("RingBuffer", () => {
  test("should initialize an empty buffer", () => {
    const rb = new RingBuffer<number>(3);
    expect(rb.isEmpty).toBe(true);
    expect(rb.size).toBe(0);
  });

  test("should enqueue and dequeue items in FIFO order", () => {
    const rb = new RingBuffer<string>(3);
    rb.push("first");
    rb.push("second");

    expect(rb.pop()).toBe("first");
    expect(rb.pop()).toBe("second");
    expect(rb.isEmpty).toBe(true);
  });

  test("should overwrite the oldest element when capacity is exceeded", () => {
    const rb = new RingBuffer<number>(3);
    rb.push(1);
    rb.push(2);
    rb.push(3);
    rb.push(4); // Overwrites 1

    expect(rb.toArray()).toEqual([2, 3, 4]);
    expect(rb.pop()).toBe(2);
  });
});
