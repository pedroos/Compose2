/*[[[cog
import cog, pathlib
ext=pathlib.Path(cog.inFile).suffix
cog.outl("""
import {{ typeTests, seqEqTests, timesTests, exceptionTests, queueTests, treeTests }} from './codetests{ext}'
// import {{ tokenTests }} from './tokentests{ext}'
""".format(ext=ext))
]]]*/

import { typeTests, seqEqTests, timesTests, exceptionTests, queueTests, treeTests } from './codetests.js'
// import { tokenTests } from './tokentests.js'

//[[[end]]]

const allTests = () => [
    typeTests,
    seqEqTests,
    timesTests,
    exceptionTests,
    queueTests,
    treeTests

    // evaluateTests(0)
]

console.log(allTests().map(t => t().join('\n')).join('\n'))

export { allTests }