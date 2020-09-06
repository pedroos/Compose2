import QtQuick 2.15
import QtQuick.Window 2.15

import "js/alltests.mjs" as AllTests

Window {
    width: 640
    height: 480
    visible: true
    title: qsTr("Hello World")

    Text {
        text: AllTests.allTests().map(t => t().join('\n')).join('\n')
    }
}
