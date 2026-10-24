export class TupleMap<K extends [string, string], V> {
  // Internally store data using a string key (e.g., "key1::key2")
  private internalMap = new Map<string, V>();
  private separator = "::";

  // Helper to serialize the [string, string] tuple into a single string
  private serializeKey(key: K): string {
    return `${key[0]}${this.separator}${key[1]}`;
  }

  // Helper to turn the internal string back into a tuple if needed
  private deserializeKey(serializedKey: string): K {
    return serializedKey.split(this.separator) as K;
  }

  set(key: K, value: V): this {
    this.internalMap.set(this.serializeKey(key), value);
    return this;
  }

  get(key: K): V | undefined {
    return this.internalMap.get(this.serializeKey(key));
  }

  has(key: K): boolean {
    return this.internalMap.has(this.serializeKey(key));
  }

  delete(key: K): boolean {
    return this.internalMap.delete(this.serializeKey(key));
  }

  clear(): void {
    this.internalMap.clear();
  }

  get size(): number {
    return this.internalMap.size;
  }

  // Custom iterator to cleanly yield [[string, string], V]
  *[Symbol.iterator](): IterableIterator<[K, V]> {
    for (const [sKey, value] of this.internalMap.entries()) {
      yield [this.deserializeKey(sKey), value];
    }
  }
}
