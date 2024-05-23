def get_right_child(index, tree):
    if index != None and (2*index)+1 < len(tree):
        return (2*index)+1
    return None
def get_left_child(index, tree):
    if index != None and (2*index) < len(tree):
        return 2*index
    return None
def postorder(index, tree, treep):
    if index != None and index >=0 and index < len(tree):
        postorder(get_left_child(index, tree), tree, treep);
        postorder(get_right_child(index, tree), tree, treep);
        treep.append(tree[index])

def parent(n, treep):
    index = treep[n-1]
    parent_index = index // 2
    if parent_index <= 0:
        return -1
    parent_node = treep.index(parent_index) + 1
    return parent_node


#    1         7
#  2   3    3    6
# 4 5 6 7  1 2  4 5
#
def solution(h, q):
    num_nodes = 2**h - 1
    tree = [n for n in range(0, num_nodes+1)]
    tree1 = [n for n in range(1, num_nodes+1)]
    treep = []
    postorder(1, tree, treep)
    #
    parents = [parent(qq, treep) for qq in q]
    return parents
