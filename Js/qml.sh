cog -r src/*.js

npm test

mkdir -p ../Qml/js
rm -f ../Qml/js/*
cp src/*.js ../Qml/js
for f in ../Qml/js/*.js; do echo $f;mv -- "$f" "${f%.js}.mjs" ; done

cog -d -r src/*.mjs