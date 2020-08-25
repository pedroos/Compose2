const types = Object.freeze({
    ElementDoesntExistException: 'ElementDoesntExistException'
})

const withType = (type, props) => ({
    ...(props !== undefined ? props(type) : {}),
    type: type
})

const isType = (obj, type) => obj.hasOwnProperty('type') && obj.type === type

const times = (t, what) => {
    const w = []
    for (let i = 0; i < t; ++i)
        w.push(what)
    return w
}

const seqEq = (seq1, seq2) => {
    if (seq1.length !== seq2.length) return false
    for (let i = 0; i < seq1.length; ++i)
        if (seq1[i] !== seq2[i]) return false
    return true
}

const ElementDoesntExistException = where =>
    withType(types.ElementDoesntExistException,
        type => ({msg: type + (where !== undefined ? " " + where : "")}))

const queue = () => {
    const items = []
    return {
        enqueue: (item) => {
            items.push(item)
        },
        dequeue: () => {
            if (!(items.length > 0)) return ElementDoesntExistException()
            const item = items[0]
            items.splice(0, 1)
            return item
        }
    }
}

// var nodePrinterDbg = (node) => times(node.level, "    ").join('') + "lvl: " + node.level + ", id: " + node.id
// var nodePrinter = (node) => times(node.level, "    ").join('') + "id: " + node.id

const recur1 = (id, path) => {
    path.push(id)
    if (id > 1)
        recur1(id - 1, path)
}

const depthFirst = (node, lvl, path) => {
    path.push({...node, level: lvl})
    if (node.children.length > 0) {
        node.children.forEach(ch => depthFirst(ch, lvl + 1, path))
    }
}

const breadthFirst = (node, lvl, path) => {
    const q = queue()
    q.enqueue({...node, level: lvl})
    let dequeued
    while (!isType(dequeued = q.dequeue(), types.ElementDoesntExistException)) {
        path.push(dequeued)
        if (dequeued.children.length > 0)
            dequeued.children.forEach(ch => q.enqueue({...ch, level: dequeued.level + 1}))
    }
}

const node = (id, children) => {
    const nd = {id: id}
    nd.children = children !== undefined ? 
        children.map(ch => ({...ch, parent: nd})) : 
        []
    return nd
}

export { types, isType, withType, times, seqEq, ElementDoesntExistException, recur1, queue, depthFirst, breadthFirst,
    node }