import QtQuick 2.15
import QtQuick.Window 2.15

//import "js/codetests.js" as CodeTests
import "js/codetests.mjs" as CodeTests

Text {
    text: CodeTests.typeTests()
}

//Window {
//    width: 640
//    height: 480
//    visible: true
//    title: qsTr("Hello World")

//    Text {
//        text: CodeTests.typeTests()
//    }
//}
