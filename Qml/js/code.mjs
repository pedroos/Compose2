function ownKeys(object, enumerableOnly) {var keys = Object.keys(object);if (Object.getOwnPropertySymbols) {var symbols = Object.getOwnPropertySymbols(object);if (enumerableOnly) symbols = symbols.filter(function (sym) {return Object.getOwnPropertyDescriptor(object, sym).enumerable;});keys.push.apply(keys, symbols);}return keys;}function _objectSpread(target) {for (var i = 1; i < arguments.length; i++) {var source = arguments[i] != null ? arguments[i] : {};if (i % 2) {ownKeys(Object(source), true).forEach(function (key) {_defineProperty(target, key, source[key]);});} else if (Object.getOwnPropertyDescriptors) {Object.defineProperties(target, Object.getOwnPropertyDescriptors(source));} else {ownKeys(Object(source)).forEach(function (key) {Object.defineProperty(target, key, Object.getOwnPropertyDescriptor(source, key));});}}return target;}function _defineProperty(obj, key, value) {if (key in obj) {Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true });} else {obj[key] = value;}return obj;}const types = Object.freeze({
  ElementDoesntExistException: 'ElementDoesntExistException' });


const withType = (type, props) => _objectSpread(_objectSpread({},
props !== undefined ? props(type) : {}), {}, {
  type: type });


const isType = (obj, type) => obj.hasOwnProperty('type') && obj.type === type;

const times = (t, what) => {
  const w = [];
  for (let i = 0; i < t; ++i)
  w.push(what);
  return w;
};

const seqEq = (seq1, seq2) => {
  if (seq1.length !== seq2.length) return false;
  for (let i = 0; i < seq1.length; ++i)
  if (seq1[i] !== seq2[i]) return false;
  return true;
};

const elementDoesntExistException = (where) =>
withType(types.ElementDoesntExistException,
type => ({ msg: type + (where !== undefined ? " " + where : "") }));

const queue = () => {
  const items = [];
  return {
    enqueue: item => {
      items.push(item);
    },
    dequeue: () => {
      if (!(items.length > 0)) return elementDoesntExistException();
      const item = items[0];
      items.splice(0, 1);
      return item;
    } };

};

// var nodePrinterDbg = (node) => times(node.level, "    ").join('') + "lvl: " + node.level + ", id: " + node.id
// var nodePrinter = (node) => times(node.level, "    ").join('') + "id: " + node.id

const recur1 = (id, path) => {
  path.push(id);
  if (id > 1)
  recur1(id - 1, path);
};

const depthFirst = (node, lvl, path) => {
  path.push(_objectSpread(_objectSpread({}, node), {}, { level: lvl }));
  if (node.children.length > 0) {
    node.children.forEach(ch => depthFirst(ch, lvl + 1, path));
  }
};

const breadthFirst = (node, lvl, path) => {
  const q = queue();
  q.enqueue(_objectSpread(_objectSpread({}, node), {}, { level: lvl }));
  let dequeued;
  while (!isType(dequeued = q.dequeue(), types.ElementDoesntExistException)) {
    path.push(dequeued);
    if (dequeued.children.length > 0)
    dequeued.children.forEach(ch => q.enqueue(_objectSpread(_objectSpread({}, ch), {}, { level: dequeued.level + 1 })));
  }
};

const node = (id, children) => {
  const nd = { id: id };
  nd.children = children !== undefined ?
  children.map(ch => _objectSpread(_objectSpread({}, ch), {}, { parent: nd })) :
  [];
  return nd;
};

export { types, isType, withType, times, seqEq, elementDoesntExistException, recur1, queue, depthFirst, breadthFirst,
node };