export class Counter<T> {
  private counts = new Map<T, number>();

  constructor(iterable?: Iterable<T>) {
    if (iterable) {
      for (const item of iterable) {
        this.add(item);
      }
    }
  }

  add(item: T, count = 1): void {
    const current = this.counts.get(item) ?? 0;
    this.counts.set(item, current + count);
  }

  get(item: T): number {
    return this.counts.get(item) ?? 0; // Returns 0 instead of undefined for missing keys
  }

  mostCommon(limit?: number): [T, number][] {
    const sorted = [...this.counts.entries()].sort((a, b) => b[1] - a[1]);
    return limit ? sorted.slice(0, limit) : sorted;
  }

  entries(): MapIterator<[T, number]> {
    return this.counts.entries();
  }

  [Symbol.iterator](): MapIterator<[T, number]> {
    return this.counts.entries();
  }
}
