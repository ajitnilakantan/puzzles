export class RingBuffer<T> {
  private buffer: (T | null)[];
  private capacity: number;
  private head = 0; // Point where we read/dequeue
  private tail = 0; // Point where we write/enqueue
  private currentSize = 0;

  constructor(capacity: number) {
    if (capacity <= 0) {
      throw new Error("Capacity must be greater than 0");
    }
    this.capacity = capacity;
    this.buffer = new Array<T | null>(capacity).fill(null);
  }

  /**
   * Adds an element to the buffer. Overwrites the oldest if full.
   */
  public push(item: T): void {
    this.buffer[this.tail] = item;

    // If full, moving the tail over the head forces the head to advance (overwriting)
    if (this.isFull) {
      this.head = (this.head + 1) % this.capacity;
    } else {
      this.currentSize++;
    }

    this.tail = (this.tail + 1) % this.capacity;
  }

  /**
   * Removes and returns the oldest element. Returns null if empty.
   */
  public pop(): T | null {
    if (this.isEmpty) {
      return null;
    }

    const item = this.buffer[this.head];
    this.buffer[this.head] = null; // Free reference for GC
    this.head = (this.head + 1) % this.capacity;
    this.currentSize--;

    return item;
  }

  /**
   * Inspects the oldest element without removing it.
   */
  public peek(): T | null {
    return this.isEmpty ? null : this.buffer[this.head];
  }

  public get size(): number {
    return this.currentSize;
  }

  public get isFull(): boolean {
    return this.currentSize === this.capacity;
  }

  public get isEmpty(): boolean {
    return this.currentSize === 0;
  }

  /**
   * Converts the valid buffer contents into a standard array (ordered oldest to newest).
   */
  public toArray(): T[] {
    const items: T[] = [];
    let current = this.head;
    for (let i = 0; i < this.currentSize; i++) {
      items.push(this.buffer[current] as T);
      current = (current + 1) % this.capacity;
    }
    return items;
  }
}
