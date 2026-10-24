import { describe, expect, test } from "bun:test";
import { Heap } from "./heap";

describe("Heap Tests", () => {
  test("Min-Heap behavior (Numbers)", () => {
    const minHeap = new Heap<number>((a, b) => a - b);

    minHeap.push(10);
    minHeap.push(5);
    minHeap.push(20);

    expect(minHeap.size()).toBe(3);
    expect(minHeap.peek()).toBe(5);
    expect(minHeap.pop()).toBe(5);
    expect(minHeap.peek()).toBe(10);
    expect(minHeap.pop()).toBe(10);
    expect(minHeap.pop()).toBe(20);
    expect(minHeap.isEmpty()).toBe(true);
  });

  test("Max-Heap behavior (Numbers)", () => {
    const maxHeap = new Heap<number>((a, b) => b - a);

    maxHeap.push(10);
    maxHeap.push(5);
    maxHeap.push(20);

    expect(maxHeap.size()).toBe(3);
    expect(maxHeap.peek()).toBe(20);
    expect(maxHeap.pop()).toBe(20);
    expect(maxHeap.peek()).toBe(10);
  });

  test("Custom Objects Priority Queue behavior", () => {
    interface Task {
      name: string;
      priority: number;
    }

    const taskQueue = new Heap<Task>((a, b) => b.priority - a.priority);

    taskQueue.push({ name: "Clean room", priority: 1 });
    taskQueue.push({ name: "Fix production bug", priority: 10 });
    taskQueue.push({ name: "Answer emails", priority: 3 });

    expect(taskQueue.size()).toBe(3);
    expect(taskQueue.peek()?.name).toBe("Fix production bug");
    expect(taskQueue.pop()?.name).toBe("Fix production bug");
    expect(taskQueue.peek()?.name).toBe("Answer emails");
  });

  test("Heap initialization with baseline array values", () => {
    const initialArray = [15, 2, 8, 1, 30];
    // Min-heap behavior
    const heap = new Heap<number>((a, b) => a - b, initialArray);

    expect(heap.size()).toBe(5);
    expect(heap.pop()).toBe(1);
    expect(heap.pop()).toBe(2);
    expect(heap.pop()).toBe(8);
  });

  test("Empty heap returns undefined on edge cases", () => {
    const emptyHeap = new Heap<number>((a, b) => a - b);

    expect(emptyHeap.peek()).toBeUndefined();
    expect(emptyHeap.pop()).toBeUndefined();
    expect(emptyHeap.isEmpty()).toBe(true);
  });
});
