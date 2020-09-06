/*[[[cog
import cog, pathlib
ext=pathlib.Path(cog.inFile).suffix
cog.outl("""
import {{ typeTests, seqEqTests, timesTests, exceptionTests, queueTests, treeTests }} from './codetests{ext}'
// import {{ tokenTests }} from './tokentests{ext}'
""".format(ext=ext))
]]]*/

import { typeTests, seqEqTests, timesTests, exceptionTests, queueTests, treeTests } from './codetests.mjs'
// import { tokenTests } from './tokentests.mjs'

//[[[end]]]

const runTests = () => {
  typeTests();
  seqEqTests();
  timesTests();
  exceptionTests();
  queueTests();
  treeTests();

  // evaluateTests(0)
};

runTests();