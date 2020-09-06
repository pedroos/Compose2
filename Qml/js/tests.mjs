/*[[[cog
import cog, pathlib
ext=pathlib.Path(cog.inFile).suffix
cog.outl("""
import {{ seqEq }} from './code{ext}'
""".format(ext=ext))
]]]*/

import { seqEq } from './code.mjs'

//[[[end]]]

const assT = (cond, name) => {if (!cond) console.log(name + " FAILED");else console.log(name + " ok");};
const assF = (cond, name) => assT(!cond, name);
const assEq = (actual, expected, name) => {if (!(actual === expected)) console.log(name + " FAILED: expected '" +
  expected + "', got '" + actual + "'");else console.log(name + " ok");};
const assSeqEq = (actual, expected, name) => {if (!seqEq(actual, expected)) console.log(name + " FAILED: seq '" +
  actual + "' differs from '" + expected + "' (expected)");else console.log(`${name} ok`);};

export { assT, assF, assEq, assSeqEq };