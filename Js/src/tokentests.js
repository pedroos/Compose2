/*[[[cog
import cog, pathlib
ext=pathlib.Path(cog.inFile).suffix
cog.outl("""
import {{ tokens, evaluate }} from './token{ext}'
import {{ assT, assF, assEq, assSeqEq }} from './tests{ext}'
""".format(ext=ext))
]]]*/

import { tokens, evaluate } from './token.js'
import { assT, assF, assEq, assSeqEq } from './tests.js'

//[[[end]]]


const evaluateTests = () => {
    const str = "(1→45, 23→ −1)"
    
    const out = []
    evaluate("wer", tokens.rule, out)
    console.log(out)
}

export { evaluateTests }