import QtQuick 2.15
import QtQuick.Window 2.15

import "js/runtests.mjs" as RunTests

Window {
    width: 640
    height: 480
    visible: true
    title: qsTr("Hello World")

    Text {
        text: RunTests.runTests()
    }
}
