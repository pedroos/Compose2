function ownKeys(object, enumerableOnly) {var keys = Object.keys(object);if (Object.getOwnPropertySymbols) {var symbols = Object.getOwnPropertySymbols(object);if (enumerableOnly) symbols = symbols.filter(function (sym) {return Object.getOwnPropertyDescriptor(object, sym).enumerable;});keys.push.apply(keys, symbols);}return keys;}function _objectSpread(target) {for (var i = 1; i < arguments.length; i++) {var source = arguments[i] != null ? arguments[i] : {};if (i % 2) {ownKeys(Object(source), true).forEach(function (key) {_defineProperty(target, key, source[key]);});} else if (Object.getOwnPropertyDescriptors) {Object.defineProperties(target, Object.getOwnPropertyDescriptors(source));} else {ownKeys(Object(source)).forEach(function (key) {Object.defineProperty(target, key, Object.getOwnPropertyDescriptor(source, key));});}}return target;}function _defineProperty(obj, key, value) {if (key in obj) {Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true });} else {obj[key] = value;}return obj;} /*[[[cog
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            import cog, pathlib
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            ext=pathlib.Path(cog.inFile).suffix
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            cog.outl("""
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            import {{ depthFirst, node }} from './code{ext}'
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            """.format(ext=ext))
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            ]]]*/

import { depthFirst, node } from './code.mjs'

//[[[end]]]

const elemType = Object.freeze({
  EitherEager: "EitherEager",
  Array: "Array",
  KeyValueItemToken: "KeyValueItemToken",
  Pair: "Pair",
  KeyValue: "KeyValue",
  Rule: "Rule",
  ExprToken: "ExprToken",
  Expr: "Expr",
  Function: "Function" });


// function uuidv4() {
//     return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
//         const r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
//         return v.toString(16);
//     });
// }

const token = children => _objectSpread({},
node(children));



const operatorType = Object.freeze({
  Infix: 'Infix',
  Suffix: 'Suffix' });


const operator = (name, operatorType) => _objectSpread(_objectSpread({},
token()), {}, {
  name: name,
  operatorType: operatorType });


const element = (elemType, children) => _objectSpread(_objectSpread({},
token(children)), {}, {
  elemType: elemType });


const tokenElement = (elemType, atom) => _objectSpread(_objectSpread({},
element(elemType)), {}, {
  atom: atom });


const atom = (name, regex) => _objectSpread(_objectSpread({},
token()), {}, {
  name: name,
  regex: regex });


const tokens = (() => {
  const and = operator('And', operatorType.Infix);
  const letter = atom('Letter', '[a-zA-Z]');
  const numeral = atom('numeral', '[0-9]');

  const array = arrayOf => element(elemType.Array, [arrayOf]);

  const keyValueItemToken = atom => tokenElement(elemType.KeyValueItemToken, atom);

  const keyValue = (keyTypeParameter, valueTypeParameter) =>
  element(elemType.KeyValue,
  [keyTypeParameter, and, valueTypeParameter]);

  const rule = element(elemType.Rule,
  [array(
  keyValue(
  array(keyValueItemToken(numeral)), array(keyValueItemToken(numeral))))]);



  return {
    rule: rule };

})();

const matchToken = (token, chr) =>
token.hasOwnProperty('elemType') ?
token.elemType === elemType.Rule && chr === '(' ||
token.elemType === elemType.Function && chr === '(' :
false;

/*
        * - Build a path of tokens
        * - For each input character:
        *   - Advance spaces
        *   - Yield tokens while they match the character (not 1:1 -- rules)
        *   - Advance character on token unmatch
        * - Return tokens
        */

const evaluate = (str, elem, out) => {
  const tokens = [];
  depthFirst(elem, 0, tokens);
  if (tokens.length === 0) return false;

  const t = 0;
  let stop = false;

  for (let i = 0; i < str.length; ++i) {
    const chr = str.charAt(i);

    while (chr === ' ')
    if (i === str.length - 1) {
      stop = true;
      break;
    }

    if (stop) break;

    while (matchToken(tokens[t], chr)) {
      out.push(tokens[t]);

      if (t === tokens.length - 1) {
        stop = true;
        break;
      }
    }

    if (stop) break;
  }

  return true;
};

export { tokens, evaluate };