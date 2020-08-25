import { tokens, evaluate } from './token.js'
import { assT, assF, assEq, assSeqEq } from './tests.js'

const evaluateTests = () => {
    const str = "(1→45, 23→ −1)"
    
    const out = []
    evaluate("wer", tokens.rule, out)
    console.log(out)
}

export { evaluateTests }