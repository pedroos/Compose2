//import { types, isType, withType, times, seqEq, ElementDoesntExistException, recur1, queue, depthFirst, breadthFirst, node } from './code.js'
//import { assT, assF, assEq, assSeqEq } from './tests.js'

.import "code.js" as Code
.import "tests.js" as Tests

.pragma library

const typeTests = () => {
    Tests.assEq(Code.withType(Code.types.ElementDoesntExistException).type,
        Code.types.ElementDoesntExistException, 'type1')
    Tests.assT(Code.isType(Code.withType(Code.types.ElementDoesntExistException),
        Code.types.ElementDoesntExistException), 'type2')
    Tests.assF(Code.isType(Code.withType(Code.types.ElementDoesntExistException),
        'sometype'), 'type3')
    Tests.assEq(Code.withType(Code.types.ElementDoesntExistException,
        type => ({typeNameUpper: type.toUpperCase()}))
        .typeNameUpper, Code.types.ElementDoesntExistException.toUpperCase(), 'type4')
}

const timesTests = () => {
    assSeqEq([' ', ' ', ' ', ' ', ' '], times(5, ' '), 'times1')
    assSeqEq([1, 1, 1, 1], times(4, 1), 'times2')
}

const seqEqTests = () => {
    Tests.assT(seqEq([1, 2, 3, 4], [1, 2, 3, 4]), 'seqEq1')
    Tests.assF(seqEq([1, 2, 3, 4], [1, 2, 3]), 'seqEq2')
    Tests.assF(seqEq([1, 2, 3, 4], [1, 2, 3, 5]), 'seqEq3')
}

const exceptionTests = () => {
    Tests.assT(Code.isType(elementDoesntExistException(), Code.types.ElementDoesntExistException),
        'exception1')
    Tests.assT(elementDoesntExistException().msg === Code.types.ElementDoesntExistException,
        'exception1b')
    Tests.assEq(elementDoesntExistException('where1').msg,
        Code.types.ElementDoesntExistException + " where1",
        'exception1c')
}

const queueTests = () => {
    const q = queue()
    q.enqueue(1)
    q.enqueue(2)
    Tests.assEq(q.dequeue(), 1, 'queue1')
    Tests.assEq(q.dequeue(), 2, 'queue1b')
    Tests.assT(Code.isType(q.dequeue(), Code.types.ElementDoesntExistException), 'queue1c')
    Tests.assEq(q.dequeue().msg, Code.types.ElementDoesntExistException, 'queue1d')
}

const treeTests = () => {
    let path = []
    recur1(10, path)
    assSeqEq(path, [10, 9, 8, 7, 6, 5, 4, 3, 2, 1], 'rec1')

    const tr1 = {
        id: 1, 
        children: [
            {
                id: 2, 
                children: [
                    {
                        id: 3, 
                        children: []
                    }, 
                    {
                        id: 4, 
                        children: []
                    }
                ]
            }, 
            {
                id: 5, 
                children: []
            }
        ]
    }

    path = []
    depthFirst(tr1, 0, path)
    assSeqEq(path.map(df => df.id), [1, 2, 3, 4, 5], 'depthFirst1')

    path = []
    breadthFirst(tr1, 0, path)
    assSeqEq(path.map(df => df.id), [1, 2, 5, 3, 4], 'breadthFirst1')

    const tr2 =
        node(0, 
            [node(1, 
                [node(2, 
                    [node(4)
                ]), 
                node(3)
            ])
        ])
    path = []
    depthFirst(tr2, 0, path)
    assSeqEq(path.map(df => df.id), [0, 1, 2, 4, 3], 'depthFirst2')

    const tr3 =
        node(0, 
            [node(1, 
                [node(2, 
                    [node(4)
                ]), 
                node(3)
            ])
        ])
    path = []
    breadthFirst(tr3, 0, path)
    assSeqEq(path.map(df => df.id), [0, 1, 2, 3, 4], 'breadthFirst2Val')
    assSeqEq(path.map(df => df.level), [0, 1, 2, 2, 3], 'breadthFirst2Lvl')

    const tr4 =
        node(0, 
            [node(1, 
                [node(2), 
                node(3, 
                    [node(4)
                ])
            ])
        ])
    path = []
    breadthFirst(tr4, 0, path)
    assSeqEq(path.map(df => df.id), [0, 1, 2, 3, 4], 'breadthFirst3Val')
    assSeqEq(path.map(df => df.level), [0, 1, 2, 2, 3], 'breadthFirst3Lvl')
}

//export { typeTests, seqEqTests, timesTests, exceptionTests, queueTests, treeTests }
