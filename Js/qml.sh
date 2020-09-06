cog -r src/*.js

rm -f ../Qml/js/*.mjs

npx babel src --out-dir ../../Qml/js --relative --out-file-extension .mjs

cog -r ../Qml/js/*.mjs

cog -d -o ../Qml/jss.qrc ../Qml/jss.qrc.template