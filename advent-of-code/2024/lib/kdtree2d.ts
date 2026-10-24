export interface Point2D {
  x: number;
  y: number;
}

export interface DistanceMetric {
  distance: (p1: Point2D, p2: Point2D) => number;
  axisDistance: (p1: Point2D, p2: Point2D, axis: 0 | 1) => number;
}

// Built-in Euclidean Metric
export const EuclideanMetric: DistanceMetric = {
  distance: (p1, p2) => Math.sqrt((p1.x - p2.x) ** 2 + (p1.y - p2.y) ** 2),
  axisDistance: (p1, p2, axis) => Math.abs(axis === 0 ? p1.x - p2.x : p1.y - p2.y),
};

// Built-in Manhattan Metric
export const ManhattanMetric: DistanceMetric = {
  distance: (p1, p2) => Math.abs(p1.x - p2.x) + Math.abs(p1.y - p2.y),
  axisDistance: (p1, p2, axis) => Math.abs(axis === 0 ? p1.x - p2.x : p1.y - p2.y),
};

export class KDNode {
  point: Point2D;
  left: KDNode | null = null;
  right: KDNode | null = null;
  axis: 0 | 1;

  constructor(point: Point2D, axis: 0 | 1) {
    this.point = point;
    this.axis = axis;
  }
}

export class KDTree2D {
  root: KDNode | null = null;
  private metric: DistanceMetric;

  // Rebalancing thresholds
  private dynamicOpcodesCount = 0;
  private rebalanceThreshold: number;

  constructor(points: Point2D[] = [], metric: DistanceMetric = EuclideanMetric, rebalanceThreshold = 1000) {
    this.metric = metric;
    this.rebalanceThreshold = rebalanceThreshold;
    if (points.length > 0) {
      this.root = this.build(points, 0);
    }
  }

  private build(points: Point2D[], depth: number): KDNode | null {
    if (points.length === 0) return null;

    const axis = (depth % 2) as 0 | 1;
    const sorted = [...points].sort((a, b) => (axis === 0 ? a.x - b.x : a.y - b.y));
    const medianIndex = Math.floor(sorted.length / 2);

    const node = new KDNode(sorted[medianIndex], axis);
    node.left = this.build(sorted.slice(0, medianIndex), depth + 1);
    node.right = this.build(sorted.slice(medianIndex + 1), depth + 1);

    return node;
  }

  /**
   * Rebuilds the entire tree into a perfectly balanced state.
   */
  public rebalance(): void {
    const points: Point2D[] = [];
    const traverse = (node: KDNode | null): void => {
      if (!node) return;
      points.push(node.point);
      traverse(node.left);
      traverse(node.right);
    };

    traverse(this.root);
    this.root = this.build(points, 0);
    this.dynamicOpcodesCount = 0;
  }

  private checkRebalance(): void {
    this.dynamicOpcodesCount++;
    if (this.dynamicOpcodesCount >= this.rebalanceThreshold) {
      this.rebalance();
    }
  }

  /**
   * Dynamically inserts a new point.
   */
  public insert(point: Point2D): void {
    const insertNode = (node: KDNode | null, depth: number): KDNode => {
      const currentAxis = (depth % 2) as 0 | 1;
      if (!node) return new KDNode(point, currentAxis);

      const coordDiff = currentAxis === 0 ? point.x - node.point.x : point.y - node.point.y;
      if (coordDiff < 0) {
        node.left = insertNode(node.left, depth + 1);
      } else {
        node.right = insertNode(node.right, depth + 1);
      }
      return node;
    };

    this.root = insertNode(this.root, 0);
    this.checkRebalance();
  }

  /**
   * Dynamically removes a specific point.
   */
  public delete(point: Point2D): void {
    const arePointsEqual = (p1: Point2D, p2: Point2D): boolean => p1.x === p2.x && p1.y === p2.y;

    const findMin = (node: KDNode | null, targetAxis: 0 | 1, depth: number): KDNode | null => {
      if (!node) return null;

      const currentAxis = (depth % 2) as 0 | 1;
      if (currentAxis === targetAxis) {
        if (!node.left) return node;
        return findMin(node.left, targetAxis, depth + 1);
      }

      // If split axis does not match target, look through both branches
      const leftMin = findMin(node.left, targetAxis, depth + 1);
      const rightMin = findMin(node.right, targetAxis, depth + 1);

      let minNode = node;
      const val = (n: KDNode): number => (targetAxis === 0 ? n.point.x : n.point.y);

      if (leftMin && val(leftMin) < val(minNode)) minNode = leftMin;
      if (rightMin && val(rightMin) < val(minNode)) minNode = rightMin;
      return minNode;
    };

    const deleteNode = (node: KDNode | null, target: Point2D, depth: number): KDNode | null => {
      if (!node) return null;

      const currentAxis = (depth % 2) as 0 | 1;

      if (arePointsEqual(node.point, target)) {
        // Case 1: Leaf Node
        if (!node.left && !node.right) return null;

        // Case 2: Has Right Subtree
        if (node.right) {
          const minNode = findMin(node.right, currentAxis, depth + 1)!;
          node.point = minNode.point;
          node.right = deleteNode(node.right, minNode.point, depth + 1);
        }
        // Case 3: Has Left Subtree Only
        else if (node.left) {
          const minNode = findMin(node.left, currentAxis, depth + 1)!;
          node.point = minNode.point;
          // Swap left to right because findMin searches left branches of matching splits
          node.right = deleteNode(node.left, minNode.point, depth + 1);
          node.left = null;
        }
        return node;
      }

      const coordDiff = currentAxis === 0 ? target.x - node.point.x : target.y - node.point.y;
      if (coordDiff < 0) {
        node.left = deleteNode(node.left, target, depth + 1);
      } else {
        node.right = deleteNode(node.right, target, depth + 1);
      }
      return node;
    };

    this.root = deleteNode(this.root, point, 0);
    this.checkRebalance();
  }

  /**
   * Spatial query method for finding nearest neighbor.
   */
  public nearest(target: Point2D): Point2D | null {
    if (!this.root) return null;
    let bestPoint = this.root.point;
    let bestDist = this.metric.distance(target, bestPoint);

    const search = (node: KDNode | null, depth: number): void => {
      if (!node) return;

      const currentDist = this.metric.distance(target, node.point);
      if (currentDist < bestDist) {
        bestDist = currentDist;
        bestPoint = node.point;
      }

      const axis = (depth % 2) as 0 | 1;
      const axisDiff = axis === 0 ? target.x - node.point.x : target.y - node.point.y;

      const firstChild = axisDiff < 0 ? node.left : node.right;
      const secondChild = axisDiff < 0 ? node.right : node.left;

      search(firstChild, depth + 1);

      const planeDist = this.metric.axisDistance(target, node.point, axis);
      if (planeDist < bestDist) {
        search(secondChild, depth + 1);
      }
    };

    search(this.root, 0);
    return bestPoint;
  }

  /**
   * Spatial query method for finding nodes within range.
   */
  public withinRange(target: Point2D, maxDistance: number): Point2D[] {
    const results: Point2D[] = [];

    const search = (node: KDNode | null, depth: number): void => {
      if (!node) return;

      const currentDist = this.metric.distance(target, node.point);
      if (currentDist <= maxDistance) {
        results.push(node.point);
      }

      const axis = (depth % 2) as 0 | 1;
      const axisDiff = axis === 0 ? target.x - node.point.x : target.y - node.point.y;

      const firstChild = axisDiff < 0 ? node.left : node.right;
      const secondChild = axisDiff < 0 ? node.right : node.left;

      search(firstChild, depth + 1);

      const planeDist = this.metric.axisDistance(target, node.point, axis);
      if (planeDist <= maxDistance) {
        search(secondChild, depth + 1);
      }
    };

    search(this.root, 0);
    return results;
  }
}
