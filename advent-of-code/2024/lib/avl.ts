export type CompareFunction<K> = (a: K, b: K) => number;

class AVLNode<K, V> {
  public left: AVLNode<K, V> | null = null;
  public right: AVLNode<K, V> | null = null;
  public height = 1;

  constructor(
    public key: K,
    public value: V
  ) {}
}

export class AVLTree<K, V> {
  private root: AVLNode<K, V> | null = null;
  private compare: CompareFunction<K>;

  constructor(compareFunction?: CompareFunction<K>) {
    this.compare =
      compareFunction ??
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      ((a: any, b: any): number => {
        if (a < b) return -1;
        if (a > b) return 1;
        return 0;
      });
  }

  /**
   * Finds the key structurally closest to the specified target key.
   * Runs in O(log n) time by traversing down a single path.
   *
   * @param targetKey The key you are matching against.
   * @param distanceFn A callback returning the absolute numerical distance between two keys.
   * @returns The closest key found, or null if the tree is completely empty.
   */
  public findClosestKey(targetKey: K, distanceFn: (a: K, b: K) => number): K | null {
    if (!this.root) return null;

    let closestNode: AVLNode<K, V> = this.root;
    let minDistance = distanceFn(targetKey, this.root.key);
    let current: AVLNode<K, V> | null = this.root;

    while (current !== null) {
      const currentDistance = distanceFn(targetKey, current.key);

      if (currentDistance < minDistance) {
        minDistance = currentDistance;
        closestNode = current;
      }

      const comparison = this.compare(targetKey, current.key);
      if (comparison < 0) {
        current = current.left;
      } else if (comparison > 0) {
        current = current.right;
      } else {
        // Found an exact key match! Distance is 0, exit immediately.
        return current.key;
      }
    }

    return closestNode.key;
  }

  // --- Static Factory ---

  public static fromArray<K, V>(
    elements: { key: K; value: V }[],
    isSorted = false,
    compareFunction?: CompareFunction<K>
  ): AVLTree<K, V> {
    const tree = new AVLTree<K, V>(compareFunction);
    if (elements.length === 0) return tree;
    if (isSorted) {
      tree.root = tree._buildFromSortedArray(elements, 0, elements.length - 1);
    } else {
      for (const element of elements) tree.insert(element.key, element.value);
    }
    return tree;
  }

  private _buildFromSortedArray(elements: { key: K; value: V }[], start: number, end: number): AVLNode<K, V> | null {
    if (start > end) return null;
    const mid = Math.floor((start + end) / 2);
    const element = elements[mid];
    const node = new AVLNode(element.key, element.value);
    node.left = this._buildFromSortedArray(elements, start, mid - 1);
    node.right = this._buildFromSortedArray(elements, mid + 1, end);
    node.height = 1 + Math.max(node.left ? node.left.height : 0, node.right ? node.right.height : 0);
    return node;
  }

  // --- Public Structural API ---

  public minKey(): K | null {
    if (!this.root) return null;
    let current = this.root;
    while (current.left !== null) current = current.left;
    return current.key;
  }

  public maxKey(): K | null {
    if (!this.root) return null;
    let current = this.root;
    while (current.right !== null) current = current.right;
    return current.key;
  }

  // --- Iterator Support ---

  public *[Symbol.iterator](): Iterator<{ key: K; value: V }> {
    yield* this._iterate(this.root);
  }

  private *_iterate(node: AVLNode<K, V> | null): Generator<{ key: K; value: V }> {
    if (node) {
      yield* this._iterate(node.left);
      yield { key: node.key, value: node.value };
      yield* this._iterate(node.right);
    }
  }

  // --- Core API ---

  public insert(key: K, value: V): void {
    this.root = this._insert(this.root, key, value);
  }

  public update(key: K, value: V): boolean {
    const node = this._find(this.root, key);
    if (node) {
      node.value = value;
      return true;
    }
    return false;
  }

  public remove(key: K): void {
    this.root = this._remove(this.root, key);
  }

  public find(key: K): V | null {
    const node = this._find(this.root, key);
    return node ? node.value : null;
  }

  public inOrderTraversal(callback: (key: K, value: V) => void): void {
    this._inOrder(this.root, callback);
  }

  // --- Internal Balanced Utilities ---

  private _getHeight(node: AVLNode<K, V> | null): number {
    return node ? node.height : 0;
  }

  private _getBalanceFactor(node: AVLNode<K, V> | null): number {
    return node ? this._getHeight(node.left) - this._getHeight(node.right) : 0;
  }

  private _updateHeight(node: AVLNode<K, V>): void {
    node.height = 1 + Math.max(this._getHeight(node.left), this._getHeight(node.right));
  }

  private _rightRotate(y: AVLNode<K, V>): AVLNode<K, V> {
    const x = y.left!;
    const T2 = x.right;
    x.right = y;
    y.left = T2;
    this._updateHeight(y);
    this._updateHeight(x);
    return x;
  }

  private _leftRotate(x: AVLNode<K, V>): AVLNode<K, V> {
    const y = x.right!;
    const T2 = y.left;
    y.left = x;
    x.right = T2;
    this._updateHeight(x);
    this._updateHeight(y);
    return y;
  }

  private _balanceNode(node: AVLNode<K, V>): AVLNode<K, V> {
    this._updateHeight(node);
    const balance = this._getBalanceFactor(node);
    if (balance > 1) {
      if (this._getBalanceFactor(node.left) < 0) node.left = this._leftRotate(node.left!);
      return this._rightRotate(node);
    }
    if (balance < -1) {
      if (this._getBalanceFactor(node.right) > 0) node.right = this._rightRotate(node.right!);
      return this._leftRotate(node);
    }
    return node;
  }

  private _insert(node: AVLNode<K, V> | null, key: K, value: V): AVLNode<K, V> {
    if (!node) return new AVLNode(key, value);
    const comparison = this.compare(key, node.key);
    if (comparison < 0) node.left = this._insert(node.left, key, value);
    else if (comparison > 0) node.right = this._insert(node.right, key, value);
    else node.value = value;
    return this._balanceNode(node);
  }

  private _find(node: AVLNode<K, V> | null, key: K): AVLNode<K, V> | null {
    if (!node) return null;
    const comparison = this.compare(key, node.key);
    if (comparison < 0) return this._find(node.left, key);
    if (comparison > 0) return this._find(node.right, key);
    return node;
  }

  private _getMinValueNode(node: AVLNode<K, V>): AVLNode<K, V> {
    let current = node;
    while (current.left !== null) current = current.left;
    return current;
  }

  private _remove(node: AVLNode<K, V> | null, key: K): AVLNode<K, V> | null {
    if (!node) return null;
    const comparison = this.compare(key, node.key);
    if (comparison < 0) node.left = this._remove(node.left, key);
    else if (comparison > 0) node.right = this._remove(node.right, key);
    else {
      if (!node.left || !node.right) node = node.left ?? node.right;
      else {
        const temp = this._getMinValueNode(node.right);
        node.key = temp.key;
        node.value = temp.value;
        node.right = this._remove(node.right, temp.key);
      }
    }
    if (!node) return null;
    return this._balanceNode(node);
  }

  private _inOrder(node: AVLNode<K, V> | null, callback: (key: K, value: V) => void): void {
    if (node) {
      this._inOrder(node.left, callback);
      callback(node.key, node.value);
      this._inOrder(node.right, callback);
    }
  }
}
