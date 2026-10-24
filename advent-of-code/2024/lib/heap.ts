export type Comparator<T> = (a: T, b: T) => number;

export class Heap<T> {
  private data: T[] = [];
  private compare: Comparator<T>;

  /**
   * @param compare A comparator function following standard Array.prototype.sort logic:
   *                - Returns negative if `a` has higher priority than `b` (moves up)
   *                - Returns positive if `b` has higher priority than `a`
   */
  constructor(compare: Comparator<T>, initialValues?: T[]) {
    this.compare = compare;
    if (initialValues) {
      this.data = [...initialValues];
      this.buildHeap();
    }
  }

  // Public API
  public size(): number {
    return this.data.length;
  }

  public isEmpty(): boolean {
    return this.data.length === 0;
  }

  public peek(): T | undefined {
    return this.data[0];
  }

  public push(value: T): void {
    this.data.push(value);
    this.heapifyUp(this.data.length - 1);
  }

  public pop(): T | undefined {
    if (this.isEmpty()) return undefined;

    const top = this.data[0];
    const bottom = this.data.pop()!;

    if (!this.isEmpty()) {
      this.data[0] = bottom;
      this.heapifyDown(0);
    }

    return top;
  }

  // Internal structural logistics
  private buildHeap(): void {
    // Start from the last non-leaf node and sift down
    for (let i = Math.floor(this.size() / 2) - 1; i >= 0; i--) {
      this.heapifyDown(i);
    }
  }

  private heapifyUp(index: number): void {
    while (index > 0) {
      const parentIdx = Math.floor((index - 1) / 2);
      if (this.compare(this.data[index], this.data[parentIdx]) < 0) {
        this.swap(index, parentIdx);
        index = parentIdx;
      } else {
        break;
      }
    }
  }

  private heapifyDown(index: number): void {
    const length = this.size();
    for (;;) {
      let prioritizedIdx = index;
      const leftIdx = 2 * index + 1;
      const rightIdx = 2 * index + 2;

      if (leftIdx < length && this.compare(this.data[leftIdx], this.data[prioritizedIdx]) < 0) {
        prioritizedIdx = leftIdx;
      }

      if (rightIdx < length && this.compare(this.data[rightIdx], this.data[prioritizedIdx]) < 0) {
        prioritizedIdx = rightIdx;
      }

      if (prioritizedIdx !== index) {
        this.swap(index, prioritizedIdx);
        index = prioritizedIdx;
      } else {
        break;
      }
    }
  }

  private swap(i: number, j: number): void {
    const temp = this.data[i];
    this.data[i] = this.data[j];
    this.data[j] = temp;
  }
}
